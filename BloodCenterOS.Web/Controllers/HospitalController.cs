using System.Text;
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
        catch { /* fall back to empty */ }

        return View(items);
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.ActiveMenu = "Hospitals";

        try
        {
            var result = await _api.GetHospitalAsync(id);
            if (result?.Success == true && result.Data != null)
                return View(result.Data);
        }
        catch { }

        TempData["Error"] = "Hospital not found";
        return RedirectToAction("Index");
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

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Hospital";
        ViewBag.ActiveMenu = "Hospitals";

        try
        {
            var result = await _api.GetHospitalAsync(id);
            if (result?.Success == true && result.Data != null)
                return View(result.Data);
        }
        catch { }

        TempData["Error"] = "Hospital not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Hospital hospital)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Hospital";
        ViewBag.ActiveMenu = "Hospitals";

        if (string.IsNullOrWhiteSpace(hospital.HospitalName))
        {
            ModelState.AddModelError("HospitalName", "Hospital name is required");
            return View(hospital);
        }

        try
        {
            var result = await _api.UpdateHospitalAsync(id, hospital);
            if (result?.Success == true)
            {
                TempData["Success"] = "Hospital updated successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to update hospital");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable. Unable to update hospital.");
        }

        return View(hospital);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");

        try
        {
            var result = await _api.DeleteHospitalAsync(id);
            if (result?.Success == true)
                TempData["Success"] = "Hospital deleted successfully";
            else
                TempData["Error"] = result?.Message ?? "Failed to delete hospital";
        }
        catch
        {
            TempData["Error"] = "API unavailable. Unable to delete hospital.";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Export()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");

        var items = new List<Hospital>();
        try
        {
            var result = await _api.GetHospitalsAsync();
            if (result?.Success == true && result.Data != null)
                items = result.Data;
        }
        catch { }

        var sb = new StringBuilder();
        sb.AppendLine("HospitalId,HospitalName,HospitalCode,Address,ContactPerson,Phone,Email,CreatedAt");
        foreach (var h in items)
        {
            sb.AppendLine($"{h.HospitalId},{EscapeCsv(h.HospitalName)},{EscapeCsv(h.HospitalCode)},{EscapeCsv(h.Address)},{EscapeCsv(h.ContactPerson)},{EscapeCsv(h.Phone)},{EscapeCsv(h.Email)},{h.CreatedAt:yyyy-MM-dd HH:mm}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"hospitals_{DateTime.Now:yyyyMMdd}.csv");
    }

    private static string EscapeCsv(string? value) =>
        string.IsNullOrEmpty(value) ? "" : $"\"{value.Replace("\"", "\"\"")}\"";
}
