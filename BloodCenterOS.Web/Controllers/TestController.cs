using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class TestController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public TestController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Testing";
        ViewBag.ActiveMenu = "Testing";

        var model = new TestListViewModel();

        try
        {
            var pending = await _api.GetPendingTestsAsync();
            if (pending?.Success == true && pending.Data != null)
                model.PendingTests = pending.Data;
        }
        catch { }

        if (!model.PendingTests.Any())
        {
            model.PendingTests = new List<BloodTestRecord>
            {
                new() { TestRecordId = 1, BagNumber = "BAG-2026-0001", OverallStatus = "Pending", CreatedAt = DateTime.Now.AddHours(-2) },
                new() { TestRecordId = 2, BagNumber = "BAG-2026-0002", OverallStatus = "Pending", CreatedAt = DateTime.Now.AddHours(-5) },
                new() { TestRecordId = 3, BagNumber = "BAG-2026-0003", OverallStatus = "Pending", CreatedAt = DateTime.Now.AddDays(-1) },
            };
        }

        model.CompletedTests = new List<BloodTestRecord>
        {
            new() { TestRecordId = 4, BagNumber = "BAG-2026-0000", OverallStatus = "Completed", CreatedAt = DateTime.Now.AddDays(-2) },
        };

        return View(model);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Test Record";
        ViewBag.ActiveMenu = "Testing";
        return View(new BloodTestRecord());
    }

    [HttpPost]
    public async Task<IActionResult> Create(BloodTestRecord record)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Test Record";
        ViewBag.ActiveMenu = "Testing";

        if (string.IsNullOrWhiteSpace(record.BagNumber))
        {
            ModelState.AddModelError("BagNumber", "Bag number is required");
            return View(record);
        }

        try
        {
            var result = await _api.CreateTestRecordAsync(record);
            if (result?.Success == true)
            {
                TempData["Success"] = $"Test record created (ID: {result.Data})";
                return RedirectToAction("ResultEntry", new { id = result.Data });
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create test record");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable.");
        }

        return View(record);
    }

    public async Task<IActionResult> ResultEntry(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Enter Test Results";
        ViewBag.ActiveMenu = "Testing";

        var model = new TestDetailViewModel();
        try
        {
            var record = await _api.GetTestRecordAsync(id);
            if (record?.Success == true && record.Data != null)
                model.Record = record.Data;
            else
                return RedirectToAction("Index");

            var results = await _api.GetTestResultsAsync(id);
            if (results?.Success == true && results.Data != null)
                model.Results = results.Data;
        }
        catch { return RedirectToAction("Index"); }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddResult(long id, string testCode, string result, string? method, string? remarks)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");

        try
        {
            var r = new BloodTestResult { TestCode = testCode, Result = result, Method = method, Remarks = remarks };
            var apiResult = await _api.AddTestResultAsync(id, r);
            if (apiResult?.Success == true)
                TempData["Success"] = $"Result for {testCode} saved";
            else
                TempData["Error"] = apiResult?.Message ?? "Failed to save result";
        }
        catch
        {
            TempData["Error"] = "API unavailable.";
        }

        return RedirectToAction("ResultEntry", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Complete(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");

        try
        {
            await _api.CompleteTestRecordAsync(id);
            TempData["Success"] = "Test record completed";
        }
        catch
        {
            TempData["Error"] = "API unavailable.";
        }

        return RedirectToAction("Index");
    }
}
