using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class DesignationRepository : IDesignationRepository
{
    private readonly IDbConnectionFactory _db;
    public DesignationRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Designation designation)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_designation_create(@p_center_id, @p_name)",
            new { p_center_id = designation.CenterId, p_name = designation.DesignationName });
    }

    public async Task UpdateAsync(Designation designation)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_designation_update(@p_designation_id, @p_name)",
            new { p_designation_id = designation.DesignationId, p_name = designation.DesignationName });
    }

    public async Task<Designation?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_designation_get_by_id(@p_designation_id)", new { p_designation_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<Designation>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_designation_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(d => d != null).Select(d => d!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_designation_delete(@p_designation_id)", new { p_designation_id = id });
    }

    private static Designation? Map(dynamic r)
    {
        if (r == null) return null;
        return new Designation
        {
            DesignationId = (long)r.designationid,
            CenterId = (long?)r.centerid,
            DesignationName = (string)r.designationname,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
