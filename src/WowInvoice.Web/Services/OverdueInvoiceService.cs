using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;
using WowInvoice.Web.Models.Entities;
using WowInvoice.Web.Models.Enums;

namespace WowInvoice.Web.Services;

public class OverdueInvoiceService(ApplicationDbContext db)
{
    public async Task<int> MarkOverdueInvoicesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var overdueInvoices = await db.Invoices
            .Where(i =>
                i.UserId == userId &&
                i.DueDate < today &&
                (i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Draft))
            .ToListAsync(cancellationToken);

        foreach (var invoice in overdueInvoices)
        {
            var previous = invoice.Status;
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTime.UtcNow;

            db.InvoiceStatusHistories.Add(new InvoiceStatusHistory
            {
                InvoiceId = invoice.Id,
                FromStatus = previous,
                ToStatus = InvoiceStatus.Overdue,
                Note = "Automatically marked overdue",
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow
            });
        }

        if (overdueInvoices.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return overdueInvoices.Count;
    }
}
