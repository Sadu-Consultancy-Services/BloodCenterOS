using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _db;
    public AuditLogRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<AuditLog>> GetAsync(long? userId, string? tableName, int limit = 100)
    {
        using var conn = _db.CreateConnection();
        var sql = "SELECT * FROM auditlog WHERE (1=1)";
        if (userId.HasValue) sql += " AND userid = @p_user_id";
        if (!string.IsNullOrEmpty(tableName)) sql += " AND tablename = @p_table_name";
        sql += " ORDER BY createdat DESC LIMIT @p_limit";
        return await conn.QueryAsync<AuditLog>(sql, new { p_user_id = userId, p_table_name = tableName, p_limit = limit });
    }
}
