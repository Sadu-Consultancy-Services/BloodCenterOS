using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _db;

    public RoleRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(long centerId, string name, string? desc, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_role_create(@p_center_id, @p_name, @p_desc, @p_created_by)",
            new { p_center_id = centerId, p_name = name, p_desc = desc, p_created_by = createdBy });
    }

    public async Task<IEnumerable<Role>> GetAllAsync(long? centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Role>(
            "SELECT * FROM fn_role_get_all(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task<IEnumerable<RolePermission>> GetPermissionsAsync(long roleId, long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<RolePermission>(
            "SELECT * FROM fn_role_permission_get_by_role(@p_role_id, @p_center_id)",
            new { p_role_id = roleId, p_center_id = centerId });
    }

    public async Task AssignPermissionAsync(long roleId, long permissionId, long centerId, long assignedBy)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_role_permission_assign(@p_role_id, @p_permission_id, @p_center_id, @p_assigned_by)",
            new { p_role_id = roleId, p_permission_id = permissionId, p_center_id = centerId, p_assigned_by = assignedBy });
    }

    public async Task RemovePermissionAsync(long roleId, long permissionId, long centerId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_role_permission_remove(@p_role_id, @p_permission_id, @p_center_id)",
            new { p_role_id = roleId, p_permission_id = permissionId, p_center_id = centerId });
    }
}
