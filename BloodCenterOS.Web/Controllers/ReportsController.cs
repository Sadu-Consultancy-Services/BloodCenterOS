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

    // ── Phase 9 Report Data Endpoints ──

    [HttpGet] public async Task<IActionResult> BloodStockData()
    {
        try { var r = await _api.GetBloodStockReportAsync(); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<BloodStockRow>());
    }

    [HttpGet] public async Task<IActionResult> ProcurementSummaryData(DateTime fromDate, DateTime toDate)
    {
        try { var r = await _api.GetProcurementSummaryAsync(fromDate, toDate); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<ProcurementSummaryRow>());
    }

    [HttpGet] public async Task<IActionResult> DonorListData(DateTime fromDate, DateTime toDate, bool showContact = true)
    {
        try { var r = await _api.GetDonorListReportAsync(fromDate, toDate, showContact); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<DonorListRow>());
    }

    [HttpGet] public async Task<IActionResult> CmIncomeData(DateTime fromDate, DateTime toDate)
    {
        try { var r = await _api.GetCmIncomeReportAsync(fromDate, toDate); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<CmIncomeRow>());
    }

    [HttpGet] public async Task<IActionResult> DiscountDetailsData(DateTime fromDate, DateTime toDate)
    {
        try { var r = await _api.GetDiscountDetailsReportAsync(fromDate, toDate); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<DiscountDetailRow>());
    }

    [HttpGet] public async Task<IActionResult> DailyIssuesData(DateTime fromDate, DateTime toDate)
    {
        try { var r = await _api.GetDailyIssuesReportAsync(fromDate, toDate); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<DailyIssueRow>());
    }

    [HttpGet] public async Task<IActionResult> MbbInwardData(DateTime fromDate, DateTime toDate, string? supplier = null)
    {
        try { var r = await _api.GetMbbInwardReportAsync(fromDate, toDate, supplier); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<MbbInwardRow>());
    }

    [HttpGet] public async Task<IActionResult> QcDailyData(DateTime date)
    {
        try { var r = await _api.GetQcDailyReportAsync(date); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<QcDailyRow>());
    }

    [HttpGet] public async Task<IActionResult> InvStockData()
    {
        try { var r = await _api.GetInvStockReportAsync(); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<InvStockRow>());
    }

    [HttpGet] public async Task<IActionResult> InvInOutData(DateTime fromDate, DateTime toDate, string? type = null, string? itemIds = null)
    {
        try { var r = await _api.GetInvInOutReportAsync(fromDate, toDate, type, itemIds); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<InvInOutRow>());
    }

    [HttpGet] public async Task<IActionResult> InvoiceDetailData(long invoiceId)
    {
        try { var r = await _api.GetInvoiceDetailReportAsync(invoiceId); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<InvoiceDetailRow>());
    }

    [HttpGet] public async Task<IActionResult> BsInvoiceDetailData(long invoiceId)
    {
        try { var r = await _api.GetBsInvoiceDetailReportAsync(invoiceId); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<BsInvoiceDetailRow>());
    }

    [HttpGet] public async Task<IActionResult> CrossMatchReportData(long invoiceId)
    {
        try { var r = await _api.GetCrossMatchReportAsync(invoiceId); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<CrossMatchReportRow>());
    }

    [HttpGet] public async Task<IActionResult> DiscardRegisterData(DateTime fromDate, DateTime toDate, string? reason = null)
    {
        try { var r = await _api.GetDiscardRegisterReportAsync(fromDate, toDate, reason); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<DiscardRegisterRow>());
    }

    [HttpGet] public async Task<IActionResult> DuesRegisterData(DateTime? asOnDate = null)
    {
        try { var r = await _api.GetDuesRegisterReportAsync(asOnDate); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<DuesRegisterRow>());
    }

    [HttpGet] public async Task<IActionResult> AutoclaveRegisterData(DateTime fromDate, DateTime toDate)
    {
        try { var r = await _api.GetAutoclaveRegisterReportAsync(fromDate, toDate); if (r?.Success == true) return Json(r.Data ?? new()); } catch { }
        return Json(new List<DiscardRegisterRow>());
    }
}
