using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class BloodReceptionController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public BloodReceptionController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Reception from MBB";
        ViewBag.ActiveMenu = "BloodReception";
        var items = new List<BloodReception>();
        try
        {
            var r = await _api.GetBloodReceptionsAsync(fromDate, toDate);
            if (r?.Success == true && r.Data != null) items = r.Data;
        }
        catch { }
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Receive Blood from MBB";
        ViewBag.ActiveMenu = "BloodReception";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string mbbName, string receiptDate, string? billNumber, string? notes,
        string? donorNames, string? sexes, string? bloodGroups, string? contactNos,
        string? bagNumbers, string? bagTypes, string? expiryDates, string? volumes)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Receive Blood from MBB";
        ViewBag.ActiveMenu = "BloodReception";

        try
        {
            var names = (donorNames ?? "").Split(',', StringSplitOptions.TrimEntries);
            var sexList = (sexes ?? "").Split(',', StringSplitOptions.TrimEntries);
            var bgList = (bloodGroups ?? "").Split(',', StringSplitOptions.TrimEntries);
            var contactList = (contactNos ?? "").Split(',', StringSplitOptions.TrimEntries);
            var bagNumList = (bagNumbers ?? "").Split(',', StringSplitOptions.TrimEntries);
            var typeList = (bagTypes ?? "").Split(',', StringSplitOptions.TrimEntries);
            var expList = (expiryDates ?? "").Split(',', StringSplitOptions.TrimEntries);
            var volList = (volumes ?? "").Split(',', StringSplitOptions.TrimEntries);

            var count = new[] { names.Length, sexList.Length, bgList.Length, contactList.Length,
                bagNumList.Length, typeList.Length, expList.Length, volList.Length }.Min();

            if (count == 0)
            {
                ModelState.AddModelError("", "At least one bag detail is required");
                return View();
            }

            var details = new List<object>();
            for (int i = 0; i < count; i++)
            {
                details.Add(new
                {
                    donorName = names.ElementAtOrDefault(i) ?? "",
                    sex = sexList.ElementAtOrDefault(i) ?? "",
                    bloodGroup = bgList.ElementAtOrDefault(i) ?? "",
                    contactNo = contactList.ElementAtOrDefault(i) ?? "",
                    bagNumber = bagNumList.ElementAtOrDefault(i) ?? "",
                    bagType = typeList.ElementAtOrDefault(i) ?? "",
                    expiryDate = expList.ElementAtOrDefault(i),
                    volumeMl = int.TryParse(volList.ElementAtOrDefault(i), out var v) ? v : 350
                });
            }

            DateTime.TryParse(receiptDate, out var rDate);

            var r = await _api.CreateBloodReceptionAsync(new
            {
                mbbName,
                receiptDate = rDate,
                billNumber,
                notes,
                details
            });
            if (r?.Success == true) { TempData["Success"] = $"{count} bags received successfully"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch (Exception ex) { ModelState.AddModelError("", $"API unavailable: {ex.Message}"); }
        return View();
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Reception Details";
        ViewBag.ActiveMenu = "BloodReception";
        try
        {
            var r = await _api.GetBloodReceptionAsync(id);
            if (r?.Success == true && r.Data != null) return View(r.Data);
        }
        catch { }
        TempData["Error"] = "Reception not found";
        return RedirectToAction("Index");
    }
}
