using System.ComponentModel.DataAnnotations;
using WowInvoice.Web.Models.Enums;

namespace WowInvoice.Web.Models.Entities;

public class Invoice : IUserOwnedEntity
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; }

    [Range(0, 999999999)]
    public decimal DiscountAmount { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<InvoiceLineItem> LineItems { get; set; } = [];
    public ICollection<InvoiceStatusHistory> StatusHistory { get; set; } = [];
}
