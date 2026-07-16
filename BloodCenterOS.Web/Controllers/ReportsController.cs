using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ReportsController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public ReportsController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public IActionResult Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Reports";
        ViewBag.ActiveMenu = "Reports";

        ViewBag.FromDate = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
        ViewBag.ToDate = DateTime.Now.ToString("yyyy-MM-dd");

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DonorData(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var q = $"/api/reports/donor-summary?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
            var result = await _api.GetAsync<List<DonorSummaryRow>>(q);
            if (result?.Success == true && result.Data != null)
                return Json(result.Data);
        }
        catch { }
        return Json(new List<DonorSummaryRow>());
    }

    [HttpGet]
    public async Task<IActionResult> InventoryData()
    {
        try
        {
            var result = await _api.GetAsync<List<InventorySummaryRow>>("/api/reports/inventory-summary");
            if (result?.Success == true && result.Data != null)
                return Json(result.Data);
        }
        catch { }
        return Json(new List<InventorySummaryRow>());
    }

    [HttpGet]
    public async Task<IActionResult> CampData(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var q = $"/api/reports/camp-summary?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
            var result = await _api.GetAsync<List<CampSummaryRow>>(q);
            if (result?.Success == true && result.Data != null)
                return Json(result.Data);
        }
        catch { }
        return Json(new List<CampSummaryRow>());
    }

    [HttpGet]
    public async Task<IActionResult> ExportDonorExcel(DateTime fromDate, DateTime toDate)
    {
        var q = $"/api/reports/export/donor-excel?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        var bytes = await _api.GetByteArrayAsync(q);
        if (bytes == null) return NotFound();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"donor_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportDonorPdf(DateTime fromDate, DateTime toDate)
    {
        var q = $"/api/reports/export/donor-pdf?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        var bytes = await _api.GetByteArrayAsync(q);
        if (bytes == null) return NotFound();
        return File(bytes, "application/pdf", $"donor_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportInventoryExcel()
    {
        var bytes = await _api.GetByteArrayAsync("/api/reports/export/inventory-excel");
        if (bytes == null) return NotFound();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"inventory_report_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportCampExcel(DateTime fromDate, DateTime toDate)
    {
        var q = $"/api/reports/export/camp-excel?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        var bytes = await _api.GetByteArrayAsync(q);
        if (bytes == null) return NotFound();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"camp_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }
}
