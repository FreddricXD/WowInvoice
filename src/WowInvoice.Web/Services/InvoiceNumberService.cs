using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;

namespace WowInvoice.Web.Services;

public class InvoiceNumberService(ApplicationDbContext db)
{
    public async Task<string> GenerateNextAsync(string userId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastNumber = await db.Invoices
            .Where(i => i.UserId == userId && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastNumber is not null)
        {
            var suffix = lastNumber[prefix.Length..];
            if (int.TryParse(suffix, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
