using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class PatientRequestController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public PatientRequestController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Patient Requests";
        ViewBag.ActiveMenu = "PatientRequests";
        var items = new List<PatientRequest>();
        try { var r = await _api.GetPatientRequestsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        var pending = new List<PatientRequest>();
        try { var r = await _api.GetPendingPatientRequestsAsync(); if (r?.Success == true && r.Data != null) pending = r.Data; } catch { }
        ViewBag.PendingCount = pending.Count;
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Patient Request";
        ViewBag.ActiveMenu = "PatientRequests";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(long? hospitalId, string patientName, int? age, string? gender, string bloodGroup, string componentType, int units, string? urgency)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Patient Request";
        ViewBag.ActiveMenu = "PatientRequests";
        try
        {
            var r = await _api.CreatePatientRequestAsync(new
            {
                hospitalId,
                patientName,
                age,
                gender,
                bloodGroup,
                componentType,
                units,
                urgency = urgency ?? "Normal"
            });
            if (r?.Success == true) { TempData["Success"] = "Patient request created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View();
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Patient Request Details";
        ViewBag.ActiveMenu = "PatientRequests";
        try { var r = await _api.GetPatientRequestAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Request not found";
        return RedirectToAction("Index");
    }
}
