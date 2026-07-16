using System.ComponentModel.DataAnnotations;
using WowInvoice.Web.Models.Enums;

namespace WowInvoice.Web.Models.ViewModels;

public class InvoiceLineItemViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 200 characters.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, 999999, ErrorMessage = "Quantity must be at least 1.")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; } = 1;

    [Required(ErrorMessage = "Unit price is required.")]
    [Range(0.01, 999999999, ErrorMessage = "Unit price must be greater than zero.")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }
}

public class InvoiceFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Please select a customer.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Issue date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Issue Date")]
    public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;

    [Required(ErrorMessage = "Due date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

    [Required(ErrorMessage = "Tax rate is required.")]
    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100.")]
    [Display(Name = "Tax Rate (%)")]
    public decimal TaxRate { get; set; }

    [Required(ErrorMessage = "Discount amount is required.")]
    [Range(0, 999999999, ErrorMessage = "Discount cannot be negative.")]
    [Display(Name = "Discount")]
    public decimal DiscountAmount { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MinLength(1, ErrorMessage = "At least one line item is required.")]
    public List<InvoiceLineItemViewModel> LineItems { get; set; } = [new()];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate.Date < IssueDate.Date)
        {
            yield return new ValidationResult(
                "Due date must be on or after the issue date.",
                [nameof(DueDate)]);
        }

        var subtotal = LineItems.Sum(i => i.Quantity * i.UnitPrice);
        if (DiscountAmount > subtotal)
        {
            yield return new ValidationResult(
                "Discount cannot exceed the invoice subtotal.",
                [nameof(DiscountAmount)]);
        }
    }
}

public class InvoiceStatusUpdateViewModel
{
    public int InvoiceId { get; set; }
    public InvoiceStatus NewStatus { get; set; }

    [StringLength(250, ErrorMessage = "Note cannot exceed 250 characters.")]
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
