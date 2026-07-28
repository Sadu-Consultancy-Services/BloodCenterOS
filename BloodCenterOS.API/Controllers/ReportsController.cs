using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reportRepo;

    public ReportsController(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet("donor-summary")]
    public async Task<IActionResult> GetDonorSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetDonorSummaryAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<DonorSummaryRow>>.Ok(data));
    }

    [HttpGet("inventory-summary")]
    public async Task<IActionResult> GetInventorySummary()
    {
        var data = await _reportRepo.GetInventorySummaryAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<InventorySummaryRow>>.Ok(data));
    }

    [HttpGet("camp-summary")]
    public async Task<IActionResult> GetCampSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetCampSummaryAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<CampSummaryRow>>.Ok(data));
    }

    [HttpGet("export/donor-excel")]
    public async Task<IActionResult> ExportDonorExcel([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetDonorSummaryAsync(CenterId, fromDate, toDate);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Donor Report");
        ws.Cell(1, 1).Value = "Period";
        ws.Cell(1, 2).Value = "Registered";
        ws.Cell(1, 3).Value = "A+";
        ws.Cell(1, 4).Value = "A-";
        ws.Cell(1, 5).Value = "B+";
        ws.Cell(1, 6).Value = "B-";
        ws.Cell(1, 7).Value = "AB+";
        ws.Cell(1, 8).Value = "AB-";
        ws.Cell(1, 9).Value = "O+";
        ws.Cell(1, 10).Value = "O-";
        ws.Cell(1, 11).Value = "Deferrals";
        ws.Cell(1, 12).Value = "Collections";

        var header = ws.Range(1, 1, 1, 12);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.Period;
            ws.Cell(row, 2).Value = item.TotalRegistered;
            ws.Cell(row, 3).Value = item.TotalBloodGroupAPositive;
            ws.Cell(row, 4).Value = item.TotalBloodGroupANegative;
            ws.Cell(row, 5).Value = item.TotalBloodGroupBPositive;
            ws.Cell(row, 6).Value = item.TotalBloodGroupBNegative;
            ws.Cell(row, 7).Value = item.TotalBloodGroupAbPositive;
            ws.Cell(row, 8).Value = item.TotalBloodGroupAbNegative;
            ws.Cell(row, 9).Value = item.TotalBloodGroupOPositive;
            ws.Cell(row, 10).Value = item.TotalBloodGroupONegative;
            ws.Cell(row, 11).Value = item.TotalDeferrals;
            ws.Cell(row, 12).Value = item.TotalCollections;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"donor_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/donor-pdf")]
    public async Task<IActionResult> ExportDonorPdf([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetDonorSummaryAsync(CenterId, fromDate, toDate);

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().AlignCenter().Text(t => t.Span($"Donor Report ({fromDate:dd MMM yyyy} - {toDate:dd MMM yyyy})").FontSize(16).Bold());
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Period").Bold();
                        h.Cell().Text("Registered").Bold();
                        h.Cell().Text("A+").Bold(); h.Cell().Text("A-").Bold();
                        h.Cell().Text("B+").Bold(); h.Cell().Text("B-").Bold();
                        h.Cell().Text("AB+").Bold(); h.Cell().Text("AB-").Bold();
                        h.Cell().Text("O+").Bold(); h.Cell().Text("O-").Bold();
                        h.Cell().Text("Deferrals").Bold();
                    });

                    foreach (var item in data)
                    {
                        table.Cell().Text(item.Period);
                        table.Cell().Text(item.TotalRegistered.ToString());
                        table.Cell().Text(item.TotalBloodGroupAPositive.ToString());
                        table.Cell().Text(item.TotalBloodGroupANegative.ToString());
                        table.Cell().Text(item.TotalBloodGroupBPositive.ToString());
                        table.Cell().Text(item.TotalBloodGroupBNegative.ToString());
                        table.Cell().Text(item.TotalBloodGroupAbPositive.ToString());
                        table.Cell().Text(item.TotalBloodGroupAbNegative.ToString());
                        table.Cell().Text(item.TotalBloodGroupOPositive.ToString());
                        table.Cell().Text(item.TotalBloodGroupONegative.ToString());
                        table.Cell().Text(item.TotalDeferrals.ToString());
                    }
                });
                page.Footer().AlignCenter().Text(x => x.CurrentPageNumber());
            });
        });

        var pdf = doc.GeneratePdf();
        return File(pdf, "application/pdf", $"donor_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet("export/inventory-excel")]
    public async Task<IActionResult> ExportInventoryExcel()
    {
        var data = await _reportRepo.GetInventorySummaryAsync(CenterId);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Inventory Report");
        ws.Cell(1, 1).Value = "Component";
        ws.Cell(1, 2).Value = "Blood Group";
        ws.Cell(1, 3).Value = "Available";
        ws.Cell(1, 4).Value = "Reserved";
        ws.Cell(1, 5).Value = "Quarantined";
        ws.Cell(1, 6).Value = "Near Expiry";

        var header = ws.Range(1, 1, 1, 6);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.ComponentType;
            ws.Cell(row, 2).Value = item.BloodGroup;
            ws.Cell(row, 3).Value = item.AvailableQty;
            ws.Cell(row, 4).Value = item.ReservedQty;
            ws.Cell(row, 5).Value = item.QuarantinedQty;
            ws.Cell(row, 6).Value = item.NearExpiryQty;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"inventory_report_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/camp-excel")]
    public async Task<IActionResult> ExportCampExcel([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetCampSummaryAsync(CenterId, fromDate, toDate);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Camp Report");
        ws.Cell(1, 1).Value = "Period";
        ws.Cell(1, 2).Value = "Camps";
        ws.Cell(1, 3).Value = "Expected";
        ws.Cell(1, 4).Value = "Collected";
        ws.Cell(1, 5).Value = "Rate (%)";

        var header = ws.Range(1, 1, 1, 5);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.Period;
            ws.Cell(row, 2).Value = item.TotalCamps;
            ws.Cell(row, 3).Value = item.TotalExpected;
            ws.Cell(row, 4).Value = item.TotalCollected;
            ws.Cell(row, 5).Value = item.CollectionRate;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"camp_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }

    // ── Phase 9: Report endpoints ──

    [HttpGet("blood-stock")]
    public async Task<IActionResult> GetBloodStock()
    {
        var data = await _reportRepo.GetBloodStockAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<BloodStockRow>>.Ok(data));
    }

    [HttpGet("procurement-summary")]
    public async Task<IActionResult> GetProcurementSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetProcurementSummaryAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<ProcurementSummaryRow>>.Ok(data));
    }

    [HttpGet("donor-list")]
    public async Task<IActionResult> GetDonorList([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] bool showContact = true)
    {
        var data = await _reportRepo.GetDonorListAsync(CenterId, fromDate, toDate, showContact);
        return Ok(ApiResponse<IEnumerable<DonorListRow>>.Ok(data));
    }

    [HttpGet("cm-income")]
    public async Task<IActionResult> GetCmIncome([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetCmIncomeAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<CmIncomeRow>>.Ok(data));
    }

    [HttpGet("discount-details")]
    public async Task<IActionResult> GetDiscountDetails([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetDiscountDetailsAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<DiscountDetailRow>>.Ok(data));
    }

    [HttpGet("daily-issues")]
    public async Task<IActionResult> GetDailyIssues([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetDailyIssuesAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<DailyIssueRow>>.Ok(data));
    }

    [HttpGet("mbb-inward")]
    public async Task<IActionResult> GetMbbInward([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? supplier)
    {
        var data = await _reportRepo.GetMbbInwardAsync(CenterId, fromDate, toDate, supplier);
        return Ok(ApiResponse<IEnumerable<MbbInwardRow>>.Ok(data));
    }

    [HttpGet("qc-daily")]
    public async Task<IActionResult> GetQcDaily([FromQuery] DateTime date)
    {
        var data = await _reportRepo.GetQcDailyAsync(CenterId, date);
        return Ok(ApiResponse<IEnumerable<QcDailyRow>>.Ok(data));
    }

    [HttpGet("inv-stock")]
    public async Task<IActionResult> GetInvStock()
    {
        var data = await _reportRepo.GetInvStockAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<InvStockRow>>.Ok(data));
    }

    [HttpGet("inv-inout")]
    public async Task<IActionResult> GetInvInOut([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? type, [FromQuery] string? itemIds)
    {
        long[]? ids = itemIds?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
        var data = await _reportRepo.GetInvInOutAsync(CenterId, fromDate, toDate, type, ids);
        return Ok(ApiResponse<IEnumerable<InvInOutRow>>.Ok(data));
    }

    [HttpGet("invoice-detail/{invoiceId}")]
    public async Task<IActionResult> GetInvoiceDetail(long invoiceId)
    {
        var data = await _reportRepo.GetInvoiceDetailAsync(CenterId, invoiceId);
        return Ok(ApiResponse<IEnumerable<InvoiceDetailRow>>.Ok(data));
    }

    [HttpGet("bs-invoice-detail/{invoiceId}")]
    public async Task<IActionResult> GetBsInvoiceDetail(long invoiceId)
    {
        var data = await _reportRepo.GetBsInvoiceDetailAsync(CenterId, invoiceId);
        return Ok(ApiResponse<IEnumerable<BsInvoiceDetailRow>>.Ok(data));
    }

    [HttpGet("crossmatch-report/{invoiceId}")]
    public async Task<IActionResult> GetCrossMatchReport(long invoiceId)
    {
        var data = await _reportRepo.GetCrossMatchReportAsync(CenterId, invoiceId);
        return Ok(ApiResponse<IEnumerable<CrossMatchReportRow>>.Ok(data));
    }

    [HttpGet("discard-register")]
    public async Task<IActionResult> GetDiscardRegister([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? reason)
    {
        var data = await _reportRepo.GetDiscardRegisterAsync(CenterId, fromDate, toDate, reason);
        return Ok(ApiResponse<IEnumerable<DiscardRegisterRow>>.Ok(data));
    }

    [HttpGet("dues-register")]
    public async Task<IActionResult> GetDuesRegister([FromQuery] DateTime? asOnDate)
    {
        var data = await _reportRepo.GetDuesRegisterAsync(CenterId, asOnDate);
        return Ok(ApiResponse<IEnumerable<DuesRegisterRow>>.Ok(data));
    }

    [HttpGet("autoclave-register")]
    public async Task<IActionResult> GetAutoclaveRegister([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var data = await _reportRepo.GetAutoclaveRegisterAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<DiscardRegisterRow>>.Ok(data));
    }
}
