using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _db;
    public NotificationRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, string type, string title, string body, string audience)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_notification_create(@p_center_id, @p_type, @p_title, @p_body, @p_audience)",
            new { p_center_id = centerId, p_type = type, p_title = title, p_body = body, p_audience = audience });
    }
}
