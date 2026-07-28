using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ReplacementDonorController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ReplacementDonorController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Replacement Donors";
        ViewBag.ActiveMenu = "ReplacementDonors";
        var items = new List<ReplacementDonor>();
        try { var r = await _api.GetReplacementDonorsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Register Replacement Donor";
        ViewBag.ActiveMenu = "ReplacementDonors";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(long requestId, long donorId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Register Replacement Donor";
        ViewBag.ActiveMenu = "ReplacementDonors";
        try
        {
            var r = await _api.RegisterReplacementDonorAsync(new { patientRequestId = requestId, donorId });
            if (r?.Success == true) { TempData["Success"] = "Replacement donor registered"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View();
    }
}
