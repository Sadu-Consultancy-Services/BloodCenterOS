using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class EmergencyController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public EmergencyController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Emergency Requests";
        ViewBag.ActiveMenu = "Emergency";

        var items = new List<EmergencyRequest>();
        try
        {
            var result = await _api.GetPendingEmergencyRequestsAsync();
            if (result?.Success == true && result.Data != null)
                items = result.Data;
        }
        catch { }

        if (!items.Any())
        {
            items = new List<EmergencyRequest>
            {
                new() { EmergencyRequestId = 1, PatientName = "Akash Verma", BloodGroup = "O-", ComponentType = "Whole Blood", UnitsRequired = 4, RequestStatus = "Critical", RequestedAt = DateTime.Now.AddMinutes(-30), HospitalId = 1 },
                new() { EmergencyRequestId = 2, PatientName = "Priya Sharma", BloodGroup = "B+", ComponentType = "PRBC", UnitsRequired = 2, RequestStatus = "Pending", RequestedAt = DateTime.Now.AddHours(-2), HospitalId = 2 },
            };
        }

        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Emergency Request";
        ViewBag.ActiveMenu = "Emergency";
        return View(new EmergencyRequest());
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmergencyRequest request)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Emergency Request";
        ViewBag.ActiveMenu = "Emergency";

        if (string.IsNullOrWhiteSpace(request.PatientName))
        {
            ModelState.AddModelError("PatientName", "Patient name is required");
            return View(request);
        }

        try
        {
            var result = await _api.CreateEmergencyRequestAsync(request);
            if (result?.Success == true)
            {
                TempData["Success"] = "Emergency request submitted";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create request");
        }
        catch { ModelState.AddModelError("", "API unavailable."); }

        return View(request);
    }
}
