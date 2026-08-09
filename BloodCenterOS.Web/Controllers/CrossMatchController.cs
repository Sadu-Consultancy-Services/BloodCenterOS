using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class CrossMatchController : Controller
{
    private readonly ApiClient _api;
    public CrossMatchController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? status, DateTime? from, DateTime? to)
    {
        ViewBag.ActiveMenu = "CrossMatch";
        var result = await _api.GetCrossMatchesAsync(status, from, to);
        var items = result?.Data ?? new();
        ViewBag.Status = status;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ActiveMenu = "CrossMatch";
        var result = await _api.GetCrossMatchPendingReservationsAsync();
        ViewBag.PendingReservations = result?.Data ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Start(long reservationId)
    {
        await _api.StartCrossMatchAsync(new { bloodRequestId = reservationId });
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(long id)
    {
        ViewBag.ActiveMenu = "CrossMatch";
        var result = await _api.GetCrossMatchAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> SetResult(long testResultId, string result)
    {
        await _api.SetCrossMatchResultAsync(new { testResultId, result });
        return RedirectToAction("Details", new { id = Request.Query["entryId"] });
    }

    [HttpPost]
    public async Task<IActionResult> RejectComponent(long testResultId)
    {
        await _api.RejectCrossMatchComponentAsync(testResultId);
        return RedirectToAction("Details", new { id = Request.Query["entryId"] });
    }
}
