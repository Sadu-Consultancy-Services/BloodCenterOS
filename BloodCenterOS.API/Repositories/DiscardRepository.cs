using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IDiscardRepository
{
    Task<IEnumerable<AvailableComponentForDiscard>> GetAvailableComponentsAsync(long centerId);
    Task<IEnumerable<DiscardRecord>> BulkDiscardAsync(long centerId, long[] componentIds, string reason, long userId, string? notes);
    Task<IEnumerable<DiscardRecord>> GetByCenterAsync(long centerId, DateTime? from, DateTime? to, string? reason);
    Task SetAutoclaveAsync(long discardId, DateTime startTime, DateTime endTime);
    Task<IEnumerable<DiscardRecord>> GetAutoclaveRegisterAsync(long centerId);
}

public class DiscardRepository : IDiscardRepository
{
    private readonly string _conn;
    public DiscardRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<AvailableComponentForDiscard>> GetAvailableComponentsAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<AvailableComponentForDiscard>(
            "SELECT * FROM fn_discard_get_available_components(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task<IEnumerable<DiscardRecord>> BulkDiscardAsync(long centerId, long[] componentIds, string reason, long userId, string? notes)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<DiscardRecord>(
            "SELECT * FROM fn_discard_bulk(@p_center_id, @p_component_ids, @p_reason, @p_discarded_by, @p_notes)",
            new { p_center_id = centerId, p_component_ids = componentIds, p_reason = reason, p_discarded_by = userId, p_notes = notes });
    }

    public async Task<IEnumerable<DiscardRecord>> GetByCenterAsync(long centerId, DateTime? from, DateTime? to, string? reason)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<DiscardRecord>(
            "SELECT * FROM fn_discard_get_by_center(@p_center_id, @p_from_date::DATE, @p_to_date::DATE, @p_reason)",
            new { p_center_id = centerId, p_from_date = from, p_to_date = to, p_reason = reason });
    }

    public async Task SetAutoclaveAsync(long discardId, DateTime startTime, DateTime endTime)
    {
        using var db = new NpgsqlConnection(_conn);
        await db.ExecuteAsync("SELECT fn_discard_set_autoclave(@p_discard_id, @p_start_time::TIMESTAMPTZ, @p_end_time::TIMESTAMPTZ)",
            new { p_discard_id = discardId, p_start_time = startTime, p_end_time = endTime });
    }

    public async Task<IEnumerable<DiscardRecord>> GetAutoclaveRegisterAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<DiscardRecord>(
            "SELECT * FROM fn_discard_get_autoclave_register(@p_center_id)",
            new { p_center_id = centerId });
    }
}
