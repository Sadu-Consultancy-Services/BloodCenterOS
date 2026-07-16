using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class HospitalController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public HospitalController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Hospitals";
        ViewBag.ActiveMenu = "Hospitals";

        var items = new List<Hospital>();

        try
        {
            var result = await _api.GetHospitalsAsync();
            if (result?.Success == true && result.Data != null)
                items = result.Data;
        }
        catch { /* fall back */ }

        if (!items.Any())
        {
            items = new List<Hospital>
            {
                new() { HospitalId = 1, HospitalName = "City General Hospital", Address = "123 MG Road, Mumbai", ContactPerson = "Dr. Sharma", Phone = "022-24567890", Email = "contact@citygen.in" },
                new() { HospitalId = 2, HospitalName = "Apex Medical Center", Address = "45 Park Avenue, Delhi", ContactPerson = "Dr. Verma", Phone = "011-23456789", Email = "info@apexmed.in" },
                new() { HospitalId = 3, HospitalName = "Sunrise Hospital & Research", Address = "88 Lake View Road, Bangalore", ContactPerson = "Dr. Nair", Phone = "080-34567890", Email = "admin@sunrise.in" },
                new() { HospitalId = 4, HospitalName = "Lifeline Super Speciality", Address = "12 Civil Lines, Pune", ContactPerson = "Dr. Joshi", Phone = "020-45678901", Email = "contact@lifeline.in" },
            };
        }

        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Hospital";
        ViewBag.ActiveMenu = "Hospitals";
        return View(new Hospital());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Hospital hospital)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Hospital";
        ViewBag.ActiveMenu = "Hospitals";

        if (string.IsNullOrWhiteSpace(hospital.HospitalName))
        {
            ModelState.AddModelError("HospitalName", "Hospital name is required");
            return View(hospital);
        }

        try
        {
            var result = await _api.CreateHospitalAsync(hospital);
            if (result?.Success == true)
            {
                TempData["Success"] = "Hospital created successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create hospital");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable. Unable to create hospital.");
        }

        return View(hospital);
    }
}
