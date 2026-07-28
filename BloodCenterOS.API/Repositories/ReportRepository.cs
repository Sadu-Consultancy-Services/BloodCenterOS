using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _db;

    public ReportRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<DonorSummaryRow>> GetDonorSummaryAsync(long centerId, DateTime fromDate, DateTime toDate)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DonorSummaryRow>(
            "SELECT * FROM fn_report_donor_summary(@p_center_id, @p_from_date, @p_to_date)",
            new { p_center_id = centerId, p_from_date = fromDate, p_to_date = toDate });
    }

    public async Task<IEnumerable<InventorySummaryRow>> GetInventorySummaryAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<InventorySummaryRow>(
            "SELECT * FROM fn_report_inventory_summary(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task<IEnumerable<CampSummaryRow>> GetCampSummaryAsync(long centerId, DateTime fromDate, DateTime toDate)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CampSummaryRow>(
            "SELECT * FROM fn_report_camp_summary(@p_center_id, @p_from_date, @p_to_date)",
            new { p_center_id = centerId, p_from_date = fromDate, p_to_date = toDate });
    }

    public async Task<IEnumerable<BloodStockRow>> GetBloodStockAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BloodStockRow>("SELECT * FROM fn_report_blood_stock(@c)", new { c = centerId });
    }

    public async Task<IEnumerable<ProcurementSummaryRow>> GetProcurementSummaryAsync(long centerId, DateTime from, DateTime to)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<ProcurementSummaryRow>(
            "SELECT * FROM fn_report_procurement_summary(@c, @f, @t)", new { c = centerId, f = from, t = to });
    }

    public async Task<IEnumerable<DonorListRow>> GetDonorListAsync(long centerId, DateTime from, DateTime to, bool showContact)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DonorListRow>(
            "SELECT * FROM fn_report_donor_list(@c, @f, @t, @s)", new { c = centerId, f = from, t = to, s = showContact });
    }

    public async Task<IEnumerable<CmIncomeRow>> GetCmIncomeAsync(long centerId, DateTime from, DateTime to)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CmIncomeRow>(
            "SELECT * FROM fn_report_cm_income(@c, @f, @t)", new { c = centerId, f = from, t = to });
    }

    public async Task<IEnumerable<DiscountDetailRow>> GetDiscountDetailsAsync(long centerId, DateTime from, DateTime to)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DiscountDetailRow>(
            "SELECT * FROM fn_report_discount_details(@c, @f, @t)", new { c = centerId, f = from, t = to });
    }

    public async Task<IEnumerable<DailyIssueRow>> GetDailyIssuesAsync(long centerId, DateTime from, DateTime to)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DailyIssueRow>(
            "SELECT * FROM fn_report_daily_issues(@c, @f, @t)", new { c = centerId, f = from, t = to });
    }

    public async Task<IEnumerable<MbbInwardRow>> GetMbbInwardAsync(long centerId, DateTime from, DateTime to, string? supplier)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<MbbInwardRow>(
            "SELECT * FROM fn_report_mbb_inward(@c, @f, @t, @s)", new { c = centerId, f = from, t = to, s = supplier ?? (object)DBNull.Value });
    }

    public async Task<IEnumerable<QcDailyRow>> GetQcDailyAsync(long centerId, DateTime date)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<QcDailyRow>(
            "SELECT * FROM fn_report_qc_daily(@c, @d)", new { c = centerId, d = date });
    }

    public async Task<IEnumerable<InvStockRow>> GetInvStockAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<InvStockRow>("SELECT * FROM fn_report_inv_stock(@c)", new { c = centerId });
    }

    public async Task<IEnumerable<InvInOutRow>> GetInvInOutAsync(long centerId, DateTime from, DateTime to, string? type, long[]? itemIds)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<InvInOutRow>(
            "SELECT * FROM fn_report_inv_inout(@c, @f, @t, @typ, @ids)",
            new { c = centerId, f = from, t = to, typ = type, ids = itemIds ?? Array.Empty<long>() });
    }

    public async Task<IEnumerable<InvoiceDetailRow>> GetInvoiceDetailAsync(long centerId, long invoiceId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<InvoiceDetailRow>(
            "SELECT * FROM fn_report_invoice_detail(@c, @i)", new { c = centerId, i = invoiceId });
    }

    public async Task<IEnumerable<BsInvoiceDetailRow>> GetBsInvoiceDetailAsync(long centerId, long invoiceId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BsInvoiceDetailRow>(
            "SELECT * FROM fn_report_bs_invoice_detail(@c, @i)", new { c = centerId, i = invoiceId });
    }

    public async Task<IEnumerable<CrossMatchReportRow>> GetCrossMatchReportAsync(long centerId, long invoiceId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CrossMatchReportRow>(
            "SELECT * FROM fn_report_crossmatch(@c, @i)", new { c = centerId, i = invoiceId });
    }

    public async Task<IEnumerable<DiscardRegisterRow>> GetDiscardRegisterAsync(long centerId, DateTime from, DateTime to, string? reason)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DiscardRegisterRow>(
            "SELECT * FROM fn_report_discard_register(@c, @f, @t, @r)",
            new { c = centerId, f = from, t = to, r = reason ?? (object)DBNull.Value });
    }

    public async Task<IEnumerable<DuesRegisterRow>> GetDuesRegisterAsync(long centerId, DateTime? asOnDate)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DuesRegisterRow>(
            "SELECT * FROM fn_report_dues_register(@c, @d)", new { c = centerId, d = asOnDate ?? DateTime.Now });
    }

    public async Task<IEnumerable<DiscardRegisterRow>> GetAutoclaveRegisterAsync(long centerId, DateTime from, DateTime to)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DiscardRegisterRow>(
            "SELECT * FROM fn_report_autoclave_register(@c, @f, @t)", new { c = centerId, f = from, t = to });
    }
}
