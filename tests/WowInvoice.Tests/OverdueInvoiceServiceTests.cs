using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;
using WowInvoice.Web.Models.Entities;
using WowInvoice.Web.Models.Enums;
using WowInvoice.Web.Services;

namespace WowInvoice.Tests;

public class OverdueInvoiceServiceTests
{
    [Fact]
    public async Task MarkOverdueInvoicesAsync_UpdatesSentInvoicesPastDueDate()
    {
        await using var db = CreateDb();
        var invoice = new Invoice
        {
            UserId = "user-1",
            InvoiceNumber = "INV-2026-0001",
            Status = InvoiceStatus.Sent,
            DueDate = DateTime.UtcNow.Date.AddDays(-5),
            CustomerId = 1,
            Customer = new Customer { UserId = "user-1", Name = "Client", Email = "c@test.com" }
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var service = new OverdueInvoiceService(db);
        var count = await service.MarkOverdueInvoicesAsync("user-1");

        Assert.Equal(1, count);
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
        Assert.Single(await db.InvoiceStatusHistories.ToListAsync());
    }

    [Fact]
    public async Task MarkOverdueInvoicesAsync_SkipsPaidInvoices()
    {
        await using var db = CreateDb();
        db.Invoices.Add(new Invoice
        {
            UserId = "user-1",
            InvoiceNumber = "INV-2026-0002",
            Status = InvoiceStatus.Paid,
            DueDate = DateTime.UtcNow.Date.AddDays(-10),
            CustomerId = 1,
            Customer = new Customer { UserId = "user-1", Name = "Client", Email = "c@test.com" }
        });
        await db.SaveChangesAsync();

        var service = new OverdueInvoiceService(db);
        var count = await service.MarkOverdueInvoicesAsync("user-1");

        Assert.Equal(0, count);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
