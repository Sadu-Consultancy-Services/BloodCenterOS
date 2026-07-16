using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class CampInventoryRepository : ICampInventoryRepository
{
    private readonly IDbConnectionFactory _db;
    public CampInventoryRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long campId, string itemName, int? quantity, string? unit)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_camp_inventory_create(@p_camp_id, @p_item_name, @p_quantity, @p_unit)",
            new { p_camp_id = campId, p_item_name = itemName, p_quantity = quantity, p_unit = unit });
    }

    public async Task UpdateAsync(long id, string? itemName, int? quantity, string? unit)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_camp_inventory_update(@p_inventory_id, @p_item_name, @p_quantity, @p_unit)",
            new { p_inventory_id = id, p_item_name = itemName, p_quantity = quantity, p_unit = unit });
    }

    public async Task<IEnumerable<CampInventory>> GetByCampAsync(long campId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_camp_inventory_get_by_camp(@p_camp_id)", new { p_camp_id = campId });
        return rows.Select(MapInventory).Where(i => i != null).Select(i => i!);
    }

    public async Task<IEnumerable<CampInventory>> GetByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_camp_inventory_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(MapInventoryWithCamp).Where(i => i != null).Select(i => i!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_camp_inventory_delete(@p_inventory_id)", new { p_inventory_id = id });
    }

    private static CampInventory? MapInventory(dynamic r)
    {
        if (r == null) return null;
        return new CampInventory
        {
            CampInventoryId = (long)r.campinventoryid,
            CampId = (long)r.campid,
            ItemName = (string?)r.itemname,
            Quantity = (int?)r.quantity,
            Unit = (string?)r.unit,
            CreatedAt = (DateTime?)r.createdat
        };
    }

    private static CampInventory? MapInventoryWithCamp(dynamic r)
    {
        if (r == null) return null;
        return new CampInventory
        {
            CampInventoryId = (long)r.campinventoryid,
            CampId = (long)r.campid,
            CampName = (string?)r.campname,
            ItemName = (string?)r.itemname,
            Quantity = (int?)r.quantity,
            Unit = (string?)r.unit,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
