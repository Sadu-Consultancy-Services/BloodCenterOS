using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class RateController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    private static readonly string[] BloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
    private static readonly string[] ComponentTypes = { "WB", "PCV", "FFP", "PC" };

    public RateController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Rate Management";
        ViewBag.ActiveMenu = "Rates";
        var items = new List<RateMaster>();
        try { var r = await _api.GetRatesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Rate";
        ViewBag.ActiveMenu = "Rates";
        ViewBag.BloodGroups = BloodGroups;
        ViewBag.ComponentTypes = ComponentTypes;
        return View(new RateMaster());
    }

    [HttpPost]
    public async Task<IActionResult> Create(RateUpsertRequest request)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Rate";
        ViewBag.ActiveMenu = "Rates";
        ViewBag.BloodGroups = BloodGroups;
        ViewBag.ComponentTypes = ComponentTypes;
        try
        {
            var r = await _api.UpsertRateAsync(new
            {
                bloodGroup = request.BloodGroup,
                componentType = request.ComponentType,
                unitRate = request.UnitRate,
                reservationRate = request.ReservationRate
            });
            if (r?.Success == true) { TempData["Success"] = "Rate saved"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        try { await _api.DeleteRateAsync(id); TempData["Success"] = "Rate deactivated"; } catch { TempData["Error"] = "Delete failed"; }
        return RedirectToAction("Index");
    }
}
