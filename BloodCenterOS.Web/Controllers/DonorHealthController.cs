using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class DonorHealthController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public DonorHealthController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(long donorId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Donor Health";
        ViewBag.ActiveMenu = "DonorHealth";
        ViewBag.DonorId = donorId;
        var items = new List<DonorHealth>();
        try { var r = await _api.GetDonorHealthAsync(donorId); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create(long donorId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Health Record";
        ViewBag.ActiveMenu = "DonorHealth";
        ViewBag.DonorId = donorId;
        return View(new DonorHealth { DonorId = donorId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(long donorId, decimal? weightKg, decimal? temperature, string? bloodPressure, decimal? hemoglobin, int? pulseRate, string? remarks)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Health Record";
        ViewBag.ActiveMenu = "DonorHealth";
        ViewBag.DonorId = donorId;
        try
        {
            var r = await _api.CreateDonorHealthAsync(donorId, new
            {
                weightKg,
                temperature,
                bloodPressure,
                hemoglobin,
                pulseRate,
                remarks
            });
            if (r?.Success == true) { TempData["Success"] = "Health record added"; return RedirectToAction("Index", new { donorId }); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(new DonorHealth { DonorId = donorId, WeightKg = weightKg, Temperature = temperature, BloodPressure = bloodPressure, Hemoglobin = hemoglobin, PulseRate = pulseRate, Remarks = remarks });
    }
}
