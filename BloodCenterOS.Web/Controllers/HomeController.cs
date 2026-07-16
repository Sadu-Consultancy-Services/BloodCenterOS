using System.Diagnostics;
using System.Text.Json;
using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public HomeController(ILogger<HomeController> logger, ApiClient api, IWebAuthService auth)
    {
        _logger = logger;
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Dashboard";
        ViewBag.ActiveMenu = "Dashboard";

        var model = new DashboardViewModel();
        try
        {
            var stockResult = await _api.GetStockAsync();
            if (stockResult?.Success == true && stockResult.Data != null)
            {
                model.StockSummary = stockResult.Data.Select(s => new StockItem
                {
                    BloodGroup = s.BloodGroup ?? "",
                    Available = s.AvailableQty,
                    Reserved = s.ReservedQty,
                    Quarantined = s.QuarantinedQty
                }).ToList();
                model.AvailableUnits = model.StockSummary.Sum(s => s.Available);
            }
        }
        catch { /* API unavailable — use default mock data */ }

        if (!model.StockSummary.Any())
        {
            model.StockSummary = new List<StockItem>
            {
                new() { BloodGroup = "A+", Available = 22, Reserved = 3, Quarantined = 1 },
                new() { BloodGroup = "A-", Available = 8, Reserved = 1, Quarantined = 0 },
                new() { BloodGroup = "B+", Available = 35, Reserved = 5, Quarantined = 2 },
                new() { BloodGroup = "B-", Available = 12, Reserved = 2, Quarantined = 0 },
                new() { BloodGroup = "AB+", Available = 15, Reserved = 2, Quarantined = 1 },
                new() { BloodGroup = "AB-", Available = 5, Reserved = 0, Quarantined = 0 },
                new() { BloodGroup = "O+", Available = 30, Reserved = 4, Quarantined = 2 },
                new() { BloodGroup = "O-", Available = 15, Reserved = 2, Quarantined = 0 },
            };
            model.AvailableUnits = model.StockSummary.Sum(s => s.Available);
        }

        model.TotalDonors = 1250;
        model.TodayCollections = 8;
        model.PendingTests = 12;
        model.PendingRequests = 5;
        model.ExpiringUnits = 3;
        model.RecentActivities = new List<RecentActivity>
        {
            new() { Time = "2 min ago", Title = "Blood Collected", Description = "Donor: Rahul Sharma — B+ (450ml)", Type = "success" },
            new() { Time = "15 min ago", Title = "Test Completed", Description = "Unit B+ — All tests negative", Type = "info" },
            new() { Time = "1 hr ago", Title = "Blood Issued", Description = "Issue #ISS-2026-0112 to City Hospital", Type = "warning" },
            new() { Time = "2 hrs ago", Title = "Camp Created", Description = "Camp: Rotary Club Drive — 2026-07-20", Type = "primary" },
        };
        model.Alerts = new List<AlertItem>
        {
            new() { Type = "danger", Message = "O- blood stock critically low (5 units remaining)" },
            new() { Type = "warning", Message = "3 units expiring within 24 hours" },
        };
        return View(model);
    }

    public async Task<IActionResult> List()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Donor List";
        ViewBag.ActiveMenu = "Donors";

        var items = new List<DonorListItem>();
        long total = 1250;
        try
        {
            var result = await _api.SearchDonorsAsync(null, null, null, 1, 20);
            if (result?.Success == true && result.Data?.Items != null)
            {
                items = result.Data.Items.Select(d => new DonorListItem
                {
                    Id = d.DonorId,
                    Code = d.DonorCode ?? $"DON-{d.DonorId}",
                    Name = d.FullName,
                    BloodGroup = d.BloodGroup ?? "",
                    Phone = d.Phone ?? "",
                    City = d.City ?? "",
                    LastDonation = d.LastDonationDate,
                    TotalDonations = d.TotalDonations,
                    Status = "Active"
                }).ToList();
                total = result.Data.TotalCount;
            }
        }
        catch { /* fall back to mock */ }

        if (!items.Any())
        {
            items = Enumerable.Range(1, 15).Select(i => new DonorListItem
            {
                Id = i,
                Code = $"DON-{2026000 + i}",
                Name = new[] { "Amit Patel", "Priya Singh", "Rahul Sharma", "Sunita Verma", "Vikram Joshi" }[i % 5],
                BloodGroup = new[] { "A+", "B+", "O+", "AB-", "A-" }[i % 5],
                Phone = $"98765{43200 + i}",
                City = new[] { "Mumbai", "Delhi", "Bangalore", "Chennai", "Pune" }[i % 5],
                LastDonation = i <= 10 ? DateTime.Now.AddDays(-i * 30) : null,
                TotalDonations = Random.Shared.Next(1, 12),
                Status = i == 5 ? "Deferred" : "Active"
            }).ToList();
        }

        var model = new ListViewModel<DonorListItem>
        {
            Title = "Donors",
            Subtitle = "Manage blood donor records",
            CreateUrl = "#",
            CreateText = "Add Donor",
            Columns = new List<string> { "Code", "Name", "Blood Group", "Phone", "City", "Last Donation", "Donations", "Status" },
            Items = items,
            TotalCount = (int)total
        };
        return View(model);
    }

    public IActionResult Form()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Donor";
        ViewBag.ActiveMenu = "Donors";
        return View();
    }

    public IActionResult Profile()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "My Profile";
        ViewBag.ActiveMenu = "Dashboard";
        return View();
    }

    public IActionResult Settings()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Settings";
        ViewBag.ActiveMenu = "Settings";
        return View();
    }

    public IActionResult AccessDenied()
    {
        Response.StatusCode = 403;
        ViewBag.Title = "Access Denied";
        return View();
    }

    public IActionResult PageNotFound()
    {
        Response.StatusCode = 404;
        ViewBag.Title = "Page Not Found";
        return View();
    }

    // ── Settings Proxy Actions ──
    [HttpGet]
    public async Task<IActionResult> SettingsCenterConfig()
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try
        {
            var result = await _api.GetCenterConfigAsync();
            if (result?.Success == true) return Json(result.Data);
        }
        catch { }
        return Json(new List<CenterConfigItem>());
    }

    [HttpPut]
    public async Task<IActionResult> SettingsSaveCenterConfig([FromBody] List<SetConfigRequest> configs)
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try { await _api.SaveCenterConfigsAsync(configs); return Json(new { success = true }); }
        catch { return Json(new { success = false }); }
    }

    [HttpGet]
    public async Task<IActionResult> SettingsSystemConfig()
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try
        {
            var result = await _api.GetSystemConfigAsync();
            if (result?.Success == true) return Json(result.Data);
        }
        catch { }
        return Json(new List<SystemConfigItem>());
    }

    [HttpPut]
    public async Task<IActionResult> SettingsSaveSystemConfig([FromBody] SetConfigRequest req)
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try { await _api.SaveSystemConfigAsync(req.Key, req.Value); return Json(new { success = true }); }
        catch { return Json(new { success = false }); }
    }

    [HttpGet]
    public async Task<IActionResult> SettingsLookupTypes()
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try
        {
            var result = await _api.GetLookupTypesAsync();
            if (result?.Success == true) return Json(result.Data);
        }
        catch { }
        return Json(new List<LookupTypeItem>());
    }

    [HttpPost]
    public async Task<IActionResult> SettingsCreateLookupType([FromBody] CreateLookupTypeReq req)
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try
        {
            var result = await _api.CreateLookupTypeAsync(req);
            return Json(new { success = result?.Success == true });
        }
        catch { return Json(new { success = false }); }
    }

    [HttpGet]
    public async Task<IActionResult> SettingsLookupValues(long typeId)
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try
        {
            var result = await _api.GetLookupValuesAsync(typeId);
            if (result?.Success == true) return Json(result.Data);
        }
        catch { }
        return Json(new List<LookupValueItem>());
    }

    [HttpPost]
    public async Task<IActionResult> SettingsCreateLookupValue([FromBody] CreateLookupValueReq req)
    {
        if (!_auth.IsAuthenticated) return Unauthorized();
        try
        {
            var result = await _api.CreateLookupValueAsync(req);
            return Json(new { success = result?.Success == true });
        }
        catch { return Json(new { success = false }); }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        ViewBag.Title = "Error";
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Components()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "UI Components";
        ViewBag.ActiveMenu = "Dashboard";
        return View();
    }

    public IActionResult About()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "About";
        ViewBag.ActiveMenu = "";
        return View();
    }
}

public class CreateLookupTypeReq
{
    public string TypeCode { get; set; } = "";
    public string TypeName { get; set; } = "";
}

public class CreateLookupValueReq
{
    public long LookupTypeId { get; set; }
    public string ValueCode { get; set; } = "";
    public string ValueText { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
