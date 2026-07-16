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
}
