using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ReturnRepository : IReturnRepository
{
    private readonly IDbConnectionFactory _db;
    public ReturnRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long issueId, long componentId, string reason, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_return_create(@p_center_id, @p_issue_id, @p_component_id, @p_reason, @p_created_by)",
            new { p_center_id = centerId, p_issue_id = issueId, p_component_id = componentId, p_reason = reason, p_created_by = createdBy });
    }

    public async Task<IEnumerable<ReturnRecord>> GetAllAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<ReturnRecord>(
            "SELECT * FROM fn_return_get_all(@p_center_id)", new { p_center_id = centerId });
    }
}
