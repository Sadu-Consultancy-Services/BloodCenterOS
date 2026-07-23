using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly IDbConnectionFactory _db;
    public LoginHistoryRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long userId, long? centerId, string? ip, string? agent)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_login_history_create(@p_user_id, @p_center_id, @p_ip, @p_agent)",
            new { p_user_id = userId, p_center_id = centerId, p_ip = ip, p_agent = agent });
    }

    public async Task LogoutAsync(long loginId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_login_history_logout(@p_login_id)", new { p_login_id = loginId });
    }

    public async Task<IEnumerable<LoginHistory>> GetFilteredAsync(long? userId, DateTime? fromDate, DateTime? toDate, int limit = 200)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<LoginHistory>(
            "SELECT * FROM fn_login_history_get_filtered(@p_user_id, @p_from_date, @p_to_date, @p_limit)",
            new { p_user_id = userId, p_from_date = fromDate, p_to_date = toDate, p_limit = limit });
    }
}
