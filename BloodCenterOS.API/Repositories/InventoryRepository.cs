using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnectionFactory _db;

    public InventoryRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> UpsertAsync(long centerId, string? componentType, string? bloodGroup, int available, int reserved, int quarantined, long? updatedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_inventory_upsert(@p_center_id, @p_component_type, @p_blood_group, @p_available, @p_reserved, @p_quarantined, @p_updated_by)",
            new
            {
                p_center_id = centerId,
                p_component_type = componentType,
                p_blood_group = bloodGroup,
                p_available = available,
                p_reserved = reserved,
                p_quarantined = quarantined,
                p_updated_by = updatedBy
            });
    }

    public async Task<IEnumerable<InventoryStock>> GetStockAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_inventory_get_stock(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new InventoryStock
        {
            ComponentType = (string?)r.componenttype,
            BloodGroup = (string?)r.bloodgroup,
            AvailableQty = (int)r.availableqty,
            ReservedQty = (int)r.reservedqty,
            QuarantinedQty = (int)r.quarantinedqty
        });
    }

    public async Task<IEnumerable<dynamic>> GetSummaryAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_inventory_get_summary(@p_center_id)",
            new { p_center_id = centerId });
    }
}
