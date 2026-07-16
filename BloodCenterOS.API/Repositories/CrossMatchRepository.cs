using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class CrossMatchRepository : ICrossMatchRepository
{
    private readonly IDbConnectionFactory _db;
    public CrossMatchRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long requestId, long componentId, string? result, string? method, long performedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_crossmatch_create(@p_center_id, @p_request_id, @p_component_id, @p_result, @p_method, @p_performed_by)",
            new { p_center_id = centerId, p_request_id = requestId, p_component_id = componentId, p_result = result, p_method = method, p_performed_by = performedBy });
    }
}
