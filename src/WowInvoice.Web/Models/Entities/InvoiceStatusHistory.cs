using WowInvoice.Web.Models.Enums;

namespace WowInvoice.Web.Models.Entities;

public class InvoiceStatusHistory
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public InvoiceStatus FromStatus { get; set; }
    public InvoiceStatus ToStatus { get; set; }

    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
}
