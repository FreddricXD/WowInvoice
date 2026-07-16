using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;
using WowInvoice.Web.Extensions;
using WowInvoice.Web.Models.Entities;
using WowInvoice.Web.Models.Enums;
using WowInvoice.Web.Models.ViewModels;
using WowInvoice.Web.Services;

namespace WowInvoice.Web.Controllers;

[Authorize]
public class InvoicesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly InvoiceNumberService _numberService;
    private readonly OverdueInvoiceService _overdueService;

    public InvoicesController(
        ApplicationDbContext db,
        InvoiceNumberService numberService,
        OverdueInvoiceService overdueService)
    {
        _db = db;
        _numberService = numberService;
        _overdueService = overdueService;
    }

    public async Task<IActionResult> Index(string? search, InvoiceStatus? status)
    {
        var userId = User.GetUserId();
        await _overdueService.MarkOverdueInvoicesAsync(userId);

        var query = _db.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.InvoiceNumber.Contains(term) ||
                i.Customer.Name.Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var invoices = await query
            .OrderByDescending(i => i.IssueDate)
            .Select(i => new InvoiceListItemViewModel
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.Customer.Name,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                Status = i.Status,
                GrandTotal = i.GrandTotal
            })
            .ToListAsync();

        ViewData["Search"] = search;
        ViewData["Status"] = status;
        return View(invoices);
    }

    public async Task<IActionResult> Create(int? customerId)
    {
        var model = new InvoiceFormViewModel();
        if (customerId.HasValue)
        {
            model.CustomerId = customerId.Value;
        }

        await PopulateCustomersAsync(model.CustomerId);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceFormViewModel model)
    {
        model.LineItems = model.LineItems.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (model.LineItems.Count == 0)
        {
            ModelState.AddModelError(nameof(model.LineItems), "At least one line item is required.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCustomersAsync(model.CustomerId);
            return View(model);
        }

        var userId = User.GetUserId();
        if (!await CustomerBelongsToUserAsync(model.CustomerId, userId))
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Invalid customer selected.");
            await PopulateCustomersAsync(model.CustomerId);
            return View(model);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var invoice = new Invoice
        {
            UserId = userId,
            CustomerId = model.CustomerId,
            InvoiceNumber = await _numberService.GenerateNextAsync(userId),
            IssueDate = model.IssueDate.Date,
            DueDate = model.DueDate.Date,
            TaxRate = model.TaxRate,
            DiscountAmount = model.DiscountAmount,
            Notes = model.Notes?.Trim(),
            Status = InvoiceStatus.Draft,
            LineItems = model.LineItems.Select(l => new InvoiceLineItem
            {
                Description = l.Description.Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        InvoiceCalculationService.Recalculate(invoice);

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        _db.InvoiceStatusHistories.Add(new InvoiceStatusHistory
        {
            InvoiceId = invoice.Id,
            FromStatus = InvoiceStatus.Draft,
            ToStatus = InvoiceStatus.Draft,
            Note = "Invoice created",
            ChangedByUserId = userId
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Success"] = $"Invoice {invoice.InvoiceNumber} created.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var invoice = await GetOwnedInvoiceForEditAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
        {
            TempData["Error"] = "Paid or cancelled invoices cannot be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await PopulateCustomersAsync(invoice.CustomerId);
        return View(MapToForm(invoice));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InvoiceFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        model.LineItems = model.LineItems.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (model.LineItems.Count == 0)
        {
            ModelState.AddModelError(nameof(model.LineItems), "At least one line item is required.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCustomersAsync(model.CustomerId);
            return View(model);
        }

        var userId = User.GetUserId();
        var invoice = await GetOwnedInvoiceForEditAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
        {
            TempData["Error"] = "Paid or cancelled invoices cannot be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!await CustomerBelongsToUserAsync(model.CustomerId, userId))
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Invalid customer selected.");
            await PopulateCustomersAsync(model.CustomerId);
            return View(model);
        }

        invoice.CustomerId = model.CustomerId;
        invoice.IssueDate = model.IssueDate.Date;
        invoice.DueDate = model.DueDate.Date;
        invoice.TaxRate = model.TaxRate;
        invoice.DiscountAmount = model.DiscountAmount;
        invoice.Notes = model.Notes?.Trim();

        _db.InvoiceLineItems.RemoveRange(invoice.LineItems);
        invoice.LineItems = model.LineItems.Select(l => new InvoiceLineItem
        {
            Description = l.Description.Trim(),
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice
        }).ToList();

        InvoiceCalculationService.Recalculate(invoice);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Invoice updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.GetUserId();
        await _overdueService.MarkOverdueInvoicesAsync(userId);

        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .Include(i => i.StatusHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (invoice is null)
        {
            return NotFound();
        }

        return View(invoice);
    }

    public async Task<IActionResult> Print(int id)
    {
        var userId = User.GetUserId();
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (invoice is null)
        {
            return NotFound();
        }

        return View(invoice);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(InvoiceStatusUpdateViewModel model)
    {
        var userId = User.GetUserId();
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(i => i.Id == model.InvoiceId && i.UserId == userId);

        if (invoice is null)
        {
            return NotFound();
        }

        if (!IsValidTransition(invoice.Status, model.NewStatus))
        {
            TempData["Error"] = $"Cannot change status from {invoice.Status} to {model.NewStatus}.";
            return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
        }

        var previous = invoice.Status;
        invoice.Status = model.NewStatus;
        invoice.UpdatedAt = DateTime.UtcNow;

        _db.InvoiceStatusHistories.Add(new InvoiceStatusHistory
        {
            InvoiceId = invoice.Id,
            FromStatus = previous,
            ToStatus = model.NewStatus,
            Note = model.Note?.Trim(),
            ChangedByUserId = userId
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Invoice marked as {model.NewStatus}.";
        return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
    }

    [HttpGet]
    public IActionResult PreviewTotals(decimal taxRate, decimal discountAmount, [FromQuery] List<int> quantities, [FromQuery] List<decimal> unitPrices)
    {
        var lines = new List<(int Quantity, decimal UnitPrice)>();
        var count = Math.Min(quantities.Count, unitPrices.Count);
        for (var i = 0; i < count; i++)
        {
            lines.Add((quantities[i], unitPrices[i]));
        }

        var result = InvoiceCalculationService.Preview(lines, taxRate, discountAmount);
        return Json(new
        {
            subtotal = result.Subtotal,
            taxAmount = result.TaxAmount,
            grandTotal = result.GrandTotal
        });
    }

    private async Task<Invoice?> GetOwnedInvoiceForEditAsync(int id)
    {
        var userId = User.GetUserId();
        return await _db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    }

    private async Task<bool> CustomerBelongsToUserAsync(int customerId, string userId) =>
        await _db.Customers.AnyAsync(c => c.Id == customerId && c.UserId == userId);

    private async Task PopulateCustomersAsync(int? selectedId = null)
    {
        var userId = User.GetUserId();
        var customers = await _db.Customers
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", selectedId);
    }

    private static InvoiceFormViewModel MapToForm(Invoice invoice) => new()
    {
        Id = invoice.Id,
        CustomerId = invoice.CustomerId,
        IssueDate = invoice.IssueDate,
        DueDate = invoice.DueDate,
        TaxRate = invoice.TaxRate,
        DiscountAmount = invoice.DiscountAmount,
        Notes = invoice.Notes,
        Status = invoice.Status,
        LineItems = invoice.LineItems.Select(l => new InvoiceLineItemViewModel
        {
            Id = l.Id,
            Description = l.Description,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice
        }).ToList()
    };

    private static bool IsValidTransition(InvoiceStatus from, InvoiceStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return from switch
        {
            InvoiceStatus.Draft => to is InvoiceStatus.Sent or InvoiceStatus.Cancelled,
            InvoiceStatus.Sent => to is InvoiceStatus.Paid or InvoiceStatus.Overdue or InvoiceStatus.Cancelled,
            InvoiceStatus.Overdue => to is InvoiceStatus.Paid or InvoiceStatus.Cancelled,
            InvoiceStatus.Paid => false,
            InvoiceStatus.Cancelled => false,
            _ => false
        };
    }
}
