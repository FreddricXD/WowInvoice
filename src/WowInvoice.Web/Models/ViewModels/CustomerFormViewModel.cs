using System.ComponentModel.DataAnnotations;

namespace WowInvoice.Web.Models.ViewModels;

public class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [Display(Name = "Full Name")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 120 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(160, ErrorMessage = "Email cannot exceed 160 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [StringLength(30, ErrorMessage = "Phone cannot exceed 30 characters.")]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
    public string? Company { get; set; }

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
    [Display(Name = "Billing Address")]
    public string? Address { get; set; }
}
