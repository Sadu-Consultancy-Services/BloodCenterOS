using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class RateRepository : IRateRepository
{
    private readonly IDbConnectionFactory _db;
    public RateRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> UpsertAsync(RateUpsertRequest request, long centerId, long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_rate_upsert(@p_center_id, @p_blood_group, @p_component_type, @p_unit_rate, @p_reservation_rate, @p_updated_by)",
            new
            {
                p_center_id = centerId,
                p_blood_group = request.BloodGroup,
                p_component_type = request.ComponentType,
                p_unit_rate = request.UnitRate,
                p_reservation_rate = request.ReservationRate,
                p_updated_by = userId
            });
    }

    public async Task<IEnumerable<RateMaster>> GetAllAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_rate_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(r => (RateMaster?)Map(r)).Where(x => x != null).Cast<RateMaster>();
    }

    public async Task<RateMaster?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var r = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_rate_get_by_id(@p_rate_id)", new { p_rate_id = id });
        return r == null ? null : Map(r);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_rate_delete(@p_rate_id)", new { p_rate_id = id });
    }

    private static RateMaster Map(dynamic r) => new()
    {
        RateId = (long)r.rateid,
        CenterId = (long)r.centerid,
        BloodGroup = (string)r.bloodgroup,
        ComponentType = (string)r.componenttype,
        UnitRate = (decimal)r.unitrate,
        ReservationRate = (decimal)r.reservationrate,
        IsActive = (bool)r.isactive,
        CreatedAt = (DateTime)r.createdat,
        UpdatedAt = (DateTime?)r.updatedat
    };
}
