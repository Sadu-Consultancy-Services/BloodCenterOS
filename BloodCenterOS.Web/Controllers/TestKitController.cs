using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class TestKitController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public TestKitController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Test Kits";
        ViewBag.ActiveMenu = "TestKits";
        var items = new List<TestKit>();
        try { var r = await _api.GetTestKitsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Test Kit";
        ViewBag.ActiveMenu = "TestKits";
        return View(new TestKit());
    }

    [HttpPost]
    public async Task<IActionResult> Create(string kitName, string? manufacturer, string? lotNumber, DateTime? expiryDate)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Test Kit";
        ViewBag.ActiveMenu = "TestKits";
        try
        {
            var r = await _api.CreateTestKitAsync(new { kitName, manufacturer, lotNumber, expiryDate });
            if (r?.Success == true) { TempData["Success"] = "Test kit created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(new TestKit { KitName = kitName ?? "", Manufacturer = manufacturer, LotNumber = lotNumber, ExpiryDate = expiryDate });
    }
}
