using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ReservationController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ReservationController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(
        string? status, DateTime? fromDate, DateTime? toDate, string? keyword)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Patient Reservations";
        ViewBag.ActiveMenu = "Reservations";
        var items = new List<BloodRequest>();
        try
        {
            var r = await _api.GetReservationsAsync(status, fromDate, toDate, keyword);
            if (r?.Success == true && r.Data != null) items = r.Data;
        }
        catch { }
        ViewBag.SelectedStatus = status;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Keyword = keyword;
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Create Reservation";
        ViewBag.ActiveMenu = "Reservations";
        return View(new ReservationCreateRequest());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReservationCreateRequest request)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Create Reservation";
        ViewBag.ActiveMenu = "Reservations";
        try
        {
            var r = await _api.CreateReservationAsync(new
            {
                patientName = request.PatientName,
                patientAddress = request.PatientAddress,
                patientContactNo = request.PatientContactNo,
                patientBloodGroup = request.PatientBloodGroup,
                requiredBloodGroup = request.RequiredBloodGroup,
                hospitalName = request.HospitalName,
                ward = request.Ward,
                componentType = request.ComponentType,
                units = request.Units,
                createInvoice = request.CreateInvoice,
                notes = request.Notes
            });
            if (r?.Success == true) { TempData["Success"] = $"{request.Units} unit(s) reserved"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch (Exception ex) { ModelState.AddModelError("", $"API unavailable: {ex.Message}"); }
        return View(request);
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Reservation Details";
        ViewBag.ActiveMenu = "Reservations";
        try
        {
            var r = await _api.GetReservationAsync(id);
            if (r?.Success == true && r.Data != null) return View(r.Data);
        }
        catch { }
        TempData["Error"] = "Reservation not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(long id, string? reason)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        try { await _api.CancelReservationAsync(id, reason); TempData["Success"] = "Reservation cancelled"; }
        catch { TempData["Error"] = "Cancel failed"; }
        return RedirectToAction("Index");
    }
}
