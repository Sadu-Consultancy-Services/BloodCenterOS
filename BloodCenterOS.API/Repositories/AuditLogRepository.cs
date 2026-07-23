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

    public async Task CreateAsync(AuditLog entry)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT fn_audit_log(" +
            "@p_property_owner_id, @p_user_id, @p_action, @p_table_name, @p_record_id, " +
            "@p_details, @p_old_val, @p_new_val, @p_ip, @p_agent)", new
        {
            p_property_owner_id = entry.PropertyOwnerId,
            p_user_id = entry.UserId,
            p_action = entry.Action,
            p_table_name = entry.TableName,
            p_record_id = entry.RecordId,
            p_details = entry.ActionDetails,
            p_old_val = entry.OldValue,
            p_new_val = entry.NewValue,
            p_ip = entry.IpAddress,
            p_agent = entry.UserAgent
        });
    }
}
