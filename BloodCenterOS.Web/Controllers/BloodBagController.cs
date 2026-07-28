using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class BloodBagController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public BloodBagController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(string? term = null)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Bags";
        ViewBag.ActiveMenu = "BloodBags";
        var items = new List<BloodBag>();
        try { var r = await _api.SearchBloodBagsAsync(term); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public async Task<IActionResult> Details(string bagNo)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Bag Details";
        ViewBag.ActiveMenu = "BloodBags";
        try { var r = await _api.GetBloodBagByNumberAsync(bagNo); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Blood bag not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(long bagId, string status)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        try
        {
            var r = await _api.UpdateBloodBagStatusAsync(bagId, status);
            if (r?.Success == true) { TempData["Success"] = "Blood bag status updated"; }
            else { TempData["Error"] = r?.Message ?? "Failed"; }
        }
        catch { TempData["Error"] = "API unavailable"; }
        return RedirectToAction("Index");
    }
}
