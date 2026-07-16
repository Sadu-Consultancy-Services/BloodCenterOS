using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ComponentLogRepository : IComponentLogRepository
{
    private readonly IDbConnectionFactory _db;
    public ComponentLogRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> StoreAsync(long centerId, long componentId, long fridgeId, string? location, string? notes)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_store(@p_center_id, @p_component_id, @p_fridge_id, @p_location, @p_notes)",
            new { p_center_id = centerId, p_component_id = componentId, p_fridge_id = fridgeId, p_location = location, p_notes = notes });
    }

    public async Task<long> TransferAsync(long centerId, long componentId, long toCenterId, string? transportDetails, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_transfer(@p_center_id, @p_component_id, @p_to_center_id, @p_transport_details, @p_created_by)",
            new { p_center_id = centerId, p_component_id = componentId, p_to_center_id = toCenterId, p_transport_details = transportDetails, p_created_by = createdBy });
    }

    public async Task<long> DiscardAsync(long centerId, long bagId, long componentId, string reason, long discardedBy, string? notes)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_discard(@p_center_id, @p_bag_id, @p_component_id, @p_reason, @p_discarded_by, @p_notes)",
            new { p_center_id = centerId, p_bag_id = bagId, p_component_id = componentId, p_reason = reason, p_discarded_by = discardedBy, p_notes = notes });
    }

    public async Task UpdateStatusAsync(long componentId, string status)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_component_update_status(@p_component_id, @p_status)", new { p_component_id = componentId, p_status = status });
    }
}
