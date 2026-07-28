using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ProcurementController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ProcurementController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(
        string? bloodGroup, string? componentType, string? status,
        DateTime? fromDate, DateTime? toDate, string? keyword)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Procurement Register";
        ViewBag.ActiveMenu = "Procurement";
        var items = new List<ProcurementRegisterItem>();
        var summary = new List<ProcurementRegisterSummaryRow>();
        try
        {
            var r = await _api.SearchProcurementRegisterAsync(bloodGroup, componentType, status, fromDate, toDate, keyword);
            if (r?.Success == true && r.Data != null) items = r.Data;

            var s = await _api.GetProcurementSummaryAsync();
            if (s?.Success == true && s.Data != null) summary = s.Data;
        }
        catch { }

        ViewBag.SelectedBloodGroup = bloodGroup;
        ViewBag.SelectedComponentType = componentType;
        ViewBag.SelectedStatus = status;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Keyword = keyword;
        ViewBag.Summary = summary;
        return View(items);
    }
}
