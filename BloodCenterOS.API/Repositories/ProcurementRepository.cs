using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ProcurementRepository : IProcurementRepository
{
    private readonly IDbConnectionFactory _db;
    public ProcurementRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<ProcurementRegisterItem>> SearchAsync(
        long centerId, string? bloodGroup, string? componentType,
        string? status, DateTime? fromDate, DateTime? toDate, string? keyword)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_procurement_register_search(@p_center_id, @p_blood_group, @p_component_type, @p_status, @p_from_date, @p_to_date, @p_keyword)",
            new
            {
                p_center_id = centerId,
                p_blood_group = bloodGroup,
                p_component_type = componentType,
                p_status = status,
                p_from_date = fromDate,
                p_to_date = toDate,
                p_keyword = keyword
            });
        return rows.Select(r => (ProcurementRegisterItem?)MapItem(r)).Where(x => x != null).Cast<ProcurementRegisterItem>();
    }

    public async Task<IEnumerable<ProcurementRegisterSummaryRow>> GetSummaryAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_procurement_register_summary(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(r => (ProcurementRegisterSummaryRow?)MapSummary(r)).Where(x => x != null).Cast<ProcurementRegisterSummaryRow>();
    }

    private static ProcurementRegisterItem? MapItem(dynamic r) => new()
    {
        RegisterId = (long)r.registerid,
        ComponentId = (long)r.componentid,
        ComponentCode = (string)r.componentcode,
        ComponentType = (string?)r.componenttype,
        VolumeMl = (int?)r.volumeml,
        BloodGroup = (string?)r.bloodgroup,
        BagNumber = (string?)r.bagnumber,
        BagType = (string?)r.bagtype,
        DonorName = (string?)r.donorname,
        DonorId = (long?)r.donorid,
        Status = (string?)r.status,
        ExpiryDate = (DateTime?)r.expirydate,
        StorageLocation = (string?)r.storagelocation,
        CreatedAt = (DateTime)r.createdat
    };

    private static ProcurementRegisterSummaryRow? MapSummary(dynamic r) => new()
    {
        BloodGroup = (string)r.bloodgroup,
        ComponentType = (string)r.componenttype,
        Available = (int)r.available,
        Reserved = (int)r.reserved,
        Issued = (int)r.issued,
        Discarded = (int)r.discarded,
        Total = (int)r.total
    };
}
