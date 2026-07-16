using System.ComponentModel.DataAnnotations;

namespace WowInvoice.Web.Models.ViewModels;

public class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required, Display(Name = "Full Name")]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone, Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    public string? Company { get; set; }

    [Display(Name = "Billing Address")]
    public string? Address { get; set; }
}
