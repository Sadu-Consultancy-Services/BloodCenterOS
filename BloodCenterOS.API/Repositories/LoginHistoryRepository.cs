using BloodCenterOS.API.Data;
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
}
