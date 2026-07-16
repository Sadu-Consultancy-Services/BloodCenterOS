using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ComponentRepository : IComponentRepository
{
    private readonly IDbConnectionFactory _db;

    public ComponentRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> PrepareAsync(long centerId, long bagId, string componentType, int volume, long preparedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_prepare(@p_center_id, @p_bag_id, @p_component_type, @p_volume, @p_prepared_by)",
            new
            {
                p_center_id = centerId,
                p_bag_id = bagId,
                p_component_type = componentType,
                p_volume = volume,
                p_prepared_by = preparedBy
            });
    }

    public async Task<IEnumerable<Component>> GetAvailableAsync(long centerId, string? bloodGroup)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_component_get_available(@p_center_id, @p_blood_group)",
            new { p_center_id = centerId, p_blood_group = bloodGroup });
        return rows.Select(r => new Component
        {
            ComponentId = (long)r.componentid,
            ComponentCode = (string)r.componentcode,
            ComponentType = (string?)r.componenttype,
            VolumeMl = (decimal?)r.volumeml,
            ExpiryDate = (DateTime?)r.expirydate,
            StorageLocation = (string?)r.storagelocation
        });
    }

    public async Task<long> TransferAsync(long centerId, long componentId, long toCenterId, string? transportDetails, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_transfer(@p_center_id, @p_component_id, @p_to_center_id, @p_transport_details, @p_created_by)",
            new
            {
                p_center_id = centerId,
                p_component_id = componentId,
                p_to_center_id = toCenterId,
                p_transport_details = transportDetails,
                p_created_by = createdBy
            });
    }

    public async Task<long> DiscardAsync(long centerId, long? bagId, long? componentId, string reason, long discardedBy, string? notes)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_discard(@p_center_id, @p_bag_id, @p_component_id, @p_reason, @p_discarded_by, @p_notes)",
            new
            {
                p_center_id = centerId,
                p_bag_id = bagId,
                p_component_id = componentId,
                p_reason = reason,
                p_discarded_by = discardedBy,
                p_notes = notes
            });
    }
}
