using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Data;
using WowInvoice.Web.Models.Enums;

namespace WowInvoice.Web.Services;

public class DashboardService(ApplicationDbContext db, OverdueInvoiceService overdueService)
{
    public async Task<DashboardViewModel> GetDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        await overdueService.MarkOverdueInvoicesAsync(userId, cancellationToken);

        var invoices = await db.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .ToListAsync(cancellationToken);

        var customers = await db.Customers
            .AsNoTracking()
            .CountAsync(c => c.UserId == userId, cancellationToken);

        var paidTotal = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.GrandTotal);
        var outstanding = invoices
            .Where(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Overdue)
            .Sum(i => i.GrandTotal);
        var overdueCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue);

        var statusBreakdown = Enum.GetValues<InvoiceStatus>()
            .Select(s => new StatusCountViewModel
            {
                Status = s,
                Count = invoices.Count(i => i.Status == s)
            })
            .Where(s => s.Count > 0)
            .ToList();

        var recentInvoices = await db.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new RecentInvoiceViewModel
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.Customer.Name,
                GrandTotal = i.GrandTotal,
                Status = i.Status,
                IssueDate = i.IssueDate
            })
            .ToListAsync(cancellationToken);

        var monthlyRevenue = invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .TakeLast(6)
            .Select(g => new MonthlyRevenueViewModel
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Amount = g.Sum(i => i.GrandTotal)
            })
            .ToList();

        return new DashboardViewModel
        {
            TotalCustomers = customers,
            TotalInvoices = invoices.Count,
            PaidRevenue = paidTotal,
            OutstandingAmount = outstanding,
            OverdueCount = overdueCount,
            StatusBreakdown = statusBreakdown,
            RecentInvoices = recentInvoices,
            MonthlyRevenue = monthlyRevenue
        };
    }
}

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalInvoices { get; set; }
    public decimal PaidRevenue { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int OverdueCount { get; set; }
    public List<StatusCountViewModel> StatusBreakdown { get; set; } = [];
    public List<RecentInvoiceViewModel> RecentInvoices { get; set; } = [];
    public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = [];
}

public class StatusCountViewModel
{
    public InvoiceStatus Status { get; set; }
    public int Count { get; set; }
}

public class RecentInvoiceViewModel
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime IssueDate { get; set; }
}

public class MonthlyRevenueViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
