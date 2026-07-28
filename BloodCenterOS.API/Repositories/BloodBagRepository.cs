using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class BloodBagRepository : IBloodBagRepository
{
    private readonly IDbConnectionFactory _db;
    public BloodBagRepository(IDbConnectionFactory db) => _db = db;

    public async Task<BloodBag?> GetByNumberAsync(string bagNo)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<BloodBag>(
            "SELECT bagid, centerid, bloodbagnumber, bagstatus, bagtype, expirydate, donorid FROM fn_bag_get_by_number(@p_bag_no)",
            new { p_bag_no = bagNo });
    }

    public async Task UpdateStatusAsync(long bagId, string status)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_bag_update_status(@p_bag_id, @p_status)", new { p_bag_id = bagId, p_status = status });
    }

    public async Task<IEnumerable<BloodBag>> SearchAsync(long centerId, string? term)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BloodBag>(
            "SELECT * FROM fn_blood_bag_search(@p_center_id, @p_term)", new { p_center_id = centerId, p_term = term });
    }
}
