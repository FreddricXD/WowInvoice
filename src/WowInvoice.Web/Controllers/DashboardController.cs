using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WowInvoice.Web.Extensions;
using WowInvoice.Web.Services;

namespace WowInvoice.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetDashboardAsync(User.GetUserId());
        return View(model);
    }
}
