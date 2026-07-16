using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class FridgeRepository : IFridgeRepository
{
    private readonly IDbConnectionFactory _db;
    public FridgeRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Fridge fridge)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_fridge_create(@p_center_id, @p_code, @p_name, @p_capacity, @p_location, @p_temp_log)",
            new { p_center_id = fridge.CenterId, p_code = fridge.FridgeCode, p_name = fridge.FridgeName, p_capacity = fridge.Capacity, p_location = fridge.Location, p_temp_log = fridge.TemperatureLogRequired });
    }

    public async Task UpdateAsync(Fridge fridge)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_fridge_update(@p_fridge_id, @p_code, @p_name, @p_capacity, @p_location, @p_temp_log)",
            new { p_fridge_id = fridge.FridgeId, p_code = fridge.FridgeCode, p_name = fridge.FridgeName, p_capacity = fridge.Capacity, p_location = fridge.Location, p_temp_log = fridge.TemperatureLogRequired });
    }

    public async Task<Fridge?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_fridge_get_by_id(@p_fridge_id)", new { p_fridge_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<Fridge>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_fridge_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(f => f != null).Select(f => f!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_fridge_delete(@p_fridge_id)", new { p_fridge_id = id });
    }

    private static Fridge? Map(dynamic r)
    {
        if (r == null) return null;
        return new Fridge
        {
            FridgeId = (long)r.fridgeid,
            CenterId = (long?)r.centerid,
            FridgeCode = (string?)r.fridgecode,
            FridgeName = (string?)r.fridgename,
            Capacity = (int?)r.capacity,
            Location = (string?)r.location,
            TemperatureLogRequired = (bool)r.temperaturelogrequired,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
