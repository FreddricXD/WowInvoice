using System.ComponentModel.DataAnnotations;
using WowInvoice.Web.Models.Enums;

namespace WowInvoice.Web.Models.ViewModels;

public class InvoiceLineItemViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999999)]
    public decimal UnitPrice { get; set; }
}

public class InvoiceFormViewModel
{
    public int? Id { get; set; }

    [Required, Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Issue Date")]
    public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

    [Range(0, 100)]
    [Display(Name = "Tax Rate (%)")]
    public decimal TaxRate { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Discount")]
    public decimal DiscountAmount { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MinLength(1, ErrorMessage = "At least one line item is required.")]
    public List<InvoiceLineItemViewModel> LineItems { get; set; } = [new()];
}

public class InvoiceStatusUpdateViewModel
{
    public int InvoiceId { get; set; }
    public InvoiceStatus NewStatus { get; set; }
    public string? Note { get; set; }
}

public class InvoiceListItemViewModel
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal GrandTotal { get; set; }
}
