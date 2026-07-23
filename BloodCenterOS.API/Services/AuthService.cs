using System.Data;
using BloodCenterOS.API.Data;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IDbConnectionFactory _db;
    private readonly IJwtService _jwt;
    private readonly ILoginHistoryRepository _loginHistory;
    private readonly IHttpContextAccessor _http;

    public AuthService(IDbConnectionFactory db, IJwtService jwt,
        ILoginHistoryRepository loginHistory, IHttpContextAccessor http)
    {
        _db = db;
        _jwt = jwt;
        _loginHistory = loginHistory;
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        using var conn = _db.CreateConnection();
        var user = await conn.QueryFirstOrDefaultAsync(
            "SELECT * FROM fn_user_get_by_username(@p_username)",
            new { p_username = request.UserName });

        if (user is null) return null;

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.passwordhash);
        if (!valid || user.islocked) return null;

        var role = await conn.QueryFirstOrDefaultAsync<string>(
            @"SELECT r.RoleName FROM UserRoleMap urm
              JOIN RoleMaster r ON r.RoleId = urm.RoleId
              WHERE urm.UserId = @id LIMIT 1",
            new { id = (long)user.userid });

        await conn.ExecuteAsync(
            "SELECT fn_user_update_login(@p_user_id)",
            new { p_user_id = (long)user.userid });

        var ctx = _http.HttpContext;
        var ip = ctx?.Connection.RemoteIpAddress;
        var ipStr = ip != null
            ? (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                ? (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString()
                    : ip.Equals(System.Net.IPAddress.IPv6Loopback) ? "127.0.0.1" : ip.ToString())
                : ip.ToString())
            : null;

        var loginId = await _loginHistory.CreateAsync(
            (long)user.userid,
            user.centerid as long?,
            ipStr,
            ctx?.Request.Headers.UserAgent.ToString());

        return new LoginResponse
        {
            Token = _jwt.GenerateToken(
                (long)user.userid,
                (string)user.username,
                role ?? "User",
                user.centerid as long? ?? 0),
            DisplayName = (string)user.displayname,
            UserId = (long)user.userid,
            Role = role ?? "User",
            LoginHistoryId = loginId
        };
    }
}
