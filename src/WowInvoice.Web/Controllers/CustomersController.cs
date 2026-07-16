using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;
using WowInvoice.Web.Extensions;
using WowInvoice.Web.Models.Entities;
using WowInvoice.Web.Models.ViewModels;

namespace WowInvoice.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;

    public CustomersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var userId = User.GetUserId();
        var query = _db.Customers
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Email.Contains(term) ||
                (c.Company != null && c.Company.Contains(term)));
        }

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerListItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Company = c.Company,
                Phone = c.Phone,
                InvoiceCount = c.Invoices.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        ViewData["Search"] = search;
        return View(customers);
    }

    public IActionResult Create() => View(new CustomerFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.GetUserId();
        var emailExists = await _db.Customers.AnyAsync(c => c.UserId == userId && c.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "A customer with this email already exists.");
            return View(model);
        }

        _db.Customers.Add(new Customer
        {
            UserId = userId,
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Phone = model.Phone?.Trim(),
            Company = model.Company?.Trim(),
            Address = model.Address?.Trim()
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await GetOwnedCustomerAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View(MapToForm(customer));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await GetOwnedCustomerAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();
        var emailTaken = await _db.Customers.AnyAsync(c =>
            c.UserId == userId && c.Email == model.Email && c.Id != id);

        if (emailTaken)
        {
            ModelState.AddModelError(nameof(model.Email), "A customer with this email already exists.");
            return View(model);
        }

        customer.Name = model.Name.Trim();
        customer.Email = model.Email.Trim();
        customer.Phone = model.Phone?.Trim();
        customer.Company = model.Company?.Trim();
        customer.Address = model.Address?.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.GetUserId();
        var customer = await _db.Customers
            .AsNoTracking()
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (customer is null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await GetOwnedCustomerAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        var hasInvoices = await _db.Invoices.AnyAsync(i => i.CustomerId == id);
        if (hasInvoices)
        {
            TempData["Error"] = "Cannot delete a customer with existing invoices.";
            return RedirectToAction(nameof(Index));
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Customer?> GetOwnedCustomerAsync(int id)
    {
        var userId = User.GetUserId();
        return await _db.Customers.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    private static CustomerFormViewModel MapToForm(Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        Company = customer.Company,
        Address = customer.Address
    };
}

public class CustomerListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public int InvoiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
