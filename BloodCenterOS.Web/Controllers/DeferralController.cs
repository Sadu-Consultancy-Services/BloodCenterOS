using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class DeferralController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public DeferralController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(long donorId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Deferrals";
        ViewBag.ActiveMenu = "Deferrals";
        ViewBag.DonorId = donorId;
        var items = new List<DeferralRecord>();
        try { var r = await _api.GetActiveDeferralsAsync(donorId); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Deferral";
        ViewBag.ActiveMenu = "Deferrals";
        return View(new DeferralRecord());
    }

    [HttpPost]
    public async Task<IActionResult> Create(long donorId, string reason, DateTime? until, string? notes)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Deferral";
        ViewBag.ActiveMenu = "Deferrals";
        try
        {
            var r = await _api.CreateDeferralAsync(new { donorId, reason, deferralUntil = until, notes });
            if (r?.Success == true) { TempData["Success"] = "Deferral created"; return RedirectToAction("Index", new { donorId }); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(new DeferralRecord { DonorId = donorId, Reason = reason, DeferralUntil = until, Notes = notes });
    }
}
