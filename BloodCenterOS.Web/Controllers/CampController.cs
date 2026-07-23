using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class CampController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public CampController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Camp Management";
        ViewBag.ActiveMenu = "Camps";

        var model = new CampListViewModel();

        try
        {
            var result = await _api.GetUpcomingCampsAsync();
            if (result?.Success == true && result.Data != null)
                model.UpcomingCamps = result.Data;
        }
        catch { /* fall back */ }

        if (!model.UpcomingCamps.Any())
        {
            model.UpcomingCamps = new List<Camp>
            {
                new() { CampId = 1, CampName = "Rotary Club Blood Drive", Venue = "Rotary Community Hall, Andheri West", City = "Mumbai", CampDate = DateTime.Now.AddDays(4), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(16), TotalDonorsExpected = 100, TotalDonorsCollected = 0 },
                new() { CampId = 2, CampName = "Lions Club Camp", Venue = "Lions Service Center, Connaught Place", City = "Delhi", CampDate = DateTime.Now.AddDays(9), StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(15), TotalDonorsExpected = 200, TotalDonorsCollected = 0 },
                new() { CampId = 3, CampName = "Corporate CSR Drive — Infosys", Venue = "Infosys Campus, Electronic City", City = "Bangalore", CampDate = DateTime.Now.AddDays(14), StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(17), TotalDonorsExpected = 150, TotalDonorsCollected = 0 },
            };
        }

        model.PastCamps = new List<Camp>
        {
            new() { CampId = 4, CampName = "Tata Memorial Hospital Drive", Venue = "Tata Hospital Premises", City = "Mumbai", CampDate = DateTime.Now.AddDays(-15), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(16), TotalDonorsExpected = 80, TotalDonorsCollected = 62 },
            new() { CampId = 5, CampName = "Radiant International School", Venue = "School Auditorium, Wakad", City = "Pune", CampDate = DateTime.Now.AddDays(-30), StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(14), TotalDonorsExpected = 120, TotalDonorsCollected = 98 },
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Create Camp";
        ViewBag.ActiveMenu = "Camps";
        try
        {
            var orgResp = await _api.GetCampOrganizersAsync();
            ViewBag.Organizers = orgResp?.Data ?? new List<CampOrganizer>();
        }
        catch
        {
            ViewBag.Organizers = new List<CampOrganizer>();
        }
        return View(new Camp());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Camp camp)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Create Camp";
        ViewBag.ActiveMenu = "Camps";

        try
        {
            var orgResp = await _api.GetCampOrganizersAsync();
            ViewBag.Organizers = orgResp?.Data ?? new List<CampOrganizer>();
        }
        catch
        {
            ViewBag.Organizers = new List<CampOrganizer>();
        }

        if (string.IsNullOrWhiteSpace(camp.CampName))
        {
            ModelState.AddModelError("CampName", "Camp name is required");
            return View(camp);
        }

        try
        {
            var result = await _api.CreateCampAsync(camp);
            if (result?.Success == true)
            {
                TempData["Success"] = "Camp created successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create camp");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable. Unable to create camp.");
        }

        return View(camp);
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.ActiveMenu = "Camps";

        var model = new CampDetailViewModel();

        try
        {
            var result = await _api.GetCampAsync(id);
            if (result?.Success == true && result.Data != null)
                model.Camp = result.Data;
            else
                return RedirectToAction("Index");
        }
        catch { return RedirectToAction("Index"); }

        model.RegisteredDonors = 0;
        model.CollectedUnits = model.Camp.TotalDonorsCollected ?? 0;
        ViewBag.Title = $"Camp — {model.Camp.CampName}";
        return View(model);
    }
}
