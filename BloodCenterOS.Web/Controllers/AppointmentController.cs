using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class AppointmentController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public AppointmentController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Appointments";
        ViewBag.ActiveMenu = "Appointments";
        var items = new List<DonorAppointment>();
        try { var r = await _api.GetAppointmentsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Appointment";
        ViewBag.ActiveMenu = "Appointments";
        return View(new DonorAppointment());
    }

    [HttpPost]
    public async Task<IActionResult> Create(long donorId, DateTime date, string? slot)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Appointment";
        ViewBag.ActiveMenu = "Appointments";
        try
        {
            var r = await _api.CreateAppointmentAsync(new { donorId, appointmentDate = date, slot });
            if (r?.Success == true) { TempData["Success"] = "Appointment created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(new DonorAppointment { DonorId = donorId, AppointmentDate = date, Slot = slot });
    }
}
