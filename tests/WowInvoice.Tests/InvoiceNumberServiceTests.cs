using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;
using WowInvoice.Web.Models.Entities;
using WowInvoice.Web.Services;

namespace WowInvoice.Tests;

public class InvoiceNumberServiceTests
{
    [Fact]
    public async Task GenerateNextAsync_StartsAtOne_WhenNoInvoicesExist()
    {
        await using var db = CreateDb();
        var service = new InvoiceNumberService(db);
        var year = DateTime.UtcNow.Year;

        var number = await service.GenerateNextAsync("user-1");

        Assert.Equal($"INV-{year}-0001", number);
    }

    [Fact]
    public async Task GenerateNextAsync_IncrementsSequence()
    {
        await using var db = CreateDb();
        var year = DateTime.UtcNow.Year;
        db.Invoices.Add(new Invoice
        {
            UserId = "user-1",
            InvoiceNumber = $"INV-{year}-0003",
            CustomerId = 1,
            Customer = new Customer { UserId = "user-1", Name = "Test", Email = "t@test.com" }
        });
        await db.SaveChangesAsync();

        var service = new InvoiceNumberService(db);
        var number = await service.GenerateNextAsync("user-1");

        Assert.Equal($"INV-{year}-0004", number);
    }

    [Fact]
    public async Task GenerateNextAsync_IsScopedPerUser()
    {
        await using var db = CreateDb();
        var year = DateTime.UtcNow.Year;
        db.Invoices.Add(new Invoice
        {
            UserId = "user-2",
            InvoiceNumber = $"INV-{year}-0010",
            CustomerId = 1,
            Customer = new Customer { UserId = "user-2", Name = "Other", Email = "o@test.com" }
        });
        await db.SaveChangesAsync();

        var service = new InvoiceNumberService(db);
        var number = await service.GenerateNextAsync("user-1");

        Assert.Equal($"INV-{year}-0001", number);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
