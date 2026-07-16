using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;

    public UserRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_user_create(@p_center_id, @p_username, @p_display_name, @p_email, @p_phone, @p_password_hash, @p_password_salt, @p_created_by)",
            new
            {
                p_center_id = user.CenterId,
                p_username = user.UserName,
                p_display_name = user.DisplayName,
                p_email = user.Email,
                p_phone = user.Phone,
                p_password_hash = user.PasswordHash,
                p_password_salt = user.PasswordSalt,
                p_created_by = user.CreatedBy
            });
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_user_get_by_id(@p_user_id)",
            new { p_user_id = id });
        if (result == null) return null;
        return new User
        {
            UserId = (long)result.userid,
            CenterId = (long?)result.centerid,
            UserName = (string)result.username,
            DisplayName = (string?)result.displayname,
            Email = (string?)result.email,
            Phone = (string?)result.phone,
            IsLocked = (bool)result.islocked,
            LastLoginAt = (DateTime?)result.lastloginat,
            CreatedAt = (DateTime)result.createdat,
            CreatedBy = (long?)result.createdby
        };
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_user_get_by_username(@p_username)",
            new { p_username = userName });
        if (result == null) return null;
        return new User
        {
            UserId = (long)result.userid,
            CenterId = (long?)result.centerid,
            UserName = (string)result.username,
            DisplayName = (string?)result.displayname,
            Email = (string?)result.email,
            Phone = (string?)result.phone,
            PasswordHash = (string?)result.passwordhash,
            PasswordSalt = (string?)result.passwordsalt,
            IsLocked = (bool)result.islocked,
            LastLoginAt = (DateTime?)result.lastloginat
        };
    }

    public async Task UpdatePasswordAsync(long userId, string hash, string salt)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_user_update_password(@p_user_id, @p_hash, @p_salt)",
            new { p_user_id = userId, p_hash = hash, p_salt = salt });
    }

    public async Task ToggleLockAsync(long userId, bool locked)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_user_toggle_lock(@p_user_id, @p_lock)",
            new { p_user_id = userId, p_lock = locked });
    }

    public async Task<IEnumerable<dynamic>> SearchAsync(long? centerId, string? keyword, int page, int size)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            "SELECT * FROM fn_user_search(@p_center_id, @p_keyword, @p_page, @p_size)",
            new { p_center_id = centerId, p_keyword = keyword, p_page = page, p_size = size });
    }

    public async Task UpdateAsync(long userId, string? displayName, string? email, string? phone, long updatedBy)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_user_update(@p_user_id, @p_display_name, @p_email, @p_phone, @p_updated_by)",
            new { p_user_id = userId, p_display_name = displayName, p_email = email, p_phone = phone, p_updated_by = updatedBy });
    }

    public async Task UpdateLoginAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_user_update_login(@p_user_id)",
            new { p_user_id = userId });
    }

    public async Task<IEnumerable<Role>> GetRolesAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Role>(
            "SELECT * FROM fn_user_role_get_by_user(@p_user_id)",
            new { p_user_id = userId });
    }

    public async Task AssignRoleAsync(long userId, long roleId, long centerId, long assignedBy)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_user_role_assign(@p_user_id, @p_role_id, @p_center_id, @p_assigned_by)",
            new { p_user_id = userId, p_role_id = roleId, p_center_id = centerId, p_assigned_by = assignedBy });
    }

    public async Task RemoveRoleAsync(long userId, long roleId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_user_role_remove(@p_user_id, @p_role_id)",
            new { p_user_id = userId, p_role_id = roleId });
    }
}
