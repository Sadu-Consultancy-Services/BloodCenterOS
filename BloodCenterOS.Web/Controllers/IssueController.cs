using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class IssueController : Controller
{
    private readonly ApiClient _api;
    public IssueController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveMenu = "Issue";
        var ready = await _api.GetReadyForIssueAsync();
        var history = await _api.GetIssueHistoryAsync();
        ViewBag.ReadyForIssue = ready?.Data ?? new();
        return View(history?.Data ?? new());
    }

    public async Task<IActionResult> Create(long? reservationId)
    {
        var ready = await _api.GetReadyForIssueAsync();
        ViewBag.ReadyForIssue = ready?.Data ?? new();
        if (reservationId.HasValue)
        {
            var selected = ready?.Data?.FirstOrDefault(r => r.ReservationId == reservationId.Value);
            ViewBag.Selected = selected;
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(long reservationId, string? paymentMode, string? notes)
    {
        var result = await _api.IssueFromReservationAsync(new { reservationId, paymentMode, notes });
        if (result?.Success == true)
        {
            TempData["Success"] = "Blood issued successfully!";
            return RedirectToAction("Index");
        }
        TempData["Error"] = result?.Message ?? "Failed to issue blood";
        return RedirectToAction("Create", new { reservationId });
    }

    public async Task<IActionResult> Details(long reservationId)
    {
        var issues = await _api.GetIssuesByReservationAsync(reservationId);
        return View(issues?.Data ?? new());
    }
}
