using System.Data;
using BloodCenterOS.API.Data;
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

    public AuthService(IDbConnectionFactory db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
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

        return new LoginResponse
        {
            Token = _jwt.GenerateToken(
                (long)user.userid,
                (string)user.username,
                role ?? "User",
                user.centerid as long? ?? 0),
            DisplayName = (string)user.displayname,
            UserId = (long)user.userid,
            Role = role ?? "User"
        };
    }
}
