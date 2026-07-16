using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class IssueController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public IssueController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Issues";
        ViewBag.ActiveMenu = "Issue";

        var pending = new List<PatientRequest>();
        var history = new List<IssueRecord>();

        try
        {
            var p = await _api.GetPendingRequestsAsync();
            if (p?.Success == true && p.Data != null) pending = p.Data;
        }
        catch { }

        try
        {
            var h = await _api.GetIssueHistoryAsync();
            if (h?.Success == true && h.Data != null) history = h.Data;
        }
        catch { }

        if (!pending.Any())
        {
            pending = new List<PatientRequest>
            {
                new() { RequestId = 1, PatientName = "Anita Deshmukh", BloodGroup = "B+", ComponentType = "PRBC", UnitsRequested = 2, RequestUrgency = "High", RequestDate = DateTime.Now.AddHours(-3) },
                new() { RequestId = 2, PatientName = "Ravi Kumar", BloodGroup = "O-", ComponentType = "Whole Blood", UnitsRequested = 3, RequestUrgency = "Critical", RequestDate = DateTime.Now.AddHours(-1) },
                new() { RequestId = 3, PatientName = "Sneha Patil", BloodGroup = "A+", ComponentType = "FFP", UnitsRequested = 1, RequestUrgency = "Normal", RequestDate = DateTime.Now.AddDays(-1) },
            };
        }

        if (!history.Any())
        {
            history = new List<IssueRecord>
            {
                new() { IssueRecordId = 1, PatientName = "Rajesh Mehta", IssueType = "Cross-match", IssueSlipNumber = "ISS-2026-001", IssueDate = DateTime.Now.AddDays(-5) },
                new() { IssueRecordId = 2, PatientName = "Meena Iyer", IssueType = "Emergency", IssueSlipNumber = "ISS-2026-002", IssueDate = DateTime.Now.AddDays(-3) },
            };
        }

        ViewBag.Pending = pending;
        return View(history);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Blood Issue";
        ViewBag.ActiveMenu = "Issue";
        return View(new IssueRecord());
    }

    [HttpPost]
    public async Task<IActionResult> Create(IssueRecord issue)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Blood Issue";
        ViewBag.ActiveMenu = "Issue";

        if (string.IsNullOrWhiteSpace(issue.PatientName))
        {
            ModelState.AddModelError("PatientName", "Patient name is required");
            return View(issue);
        }

        try
        {
            var result = await _api.CreateIssueAsync(issue);
            if (result?.Success == true)
            {
                TempData["Success"] = "Issue created successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create issue");
        }
        catch { ModelState.AddModelError("", "API unavailable."); }

        return View(issue);
    }
}
