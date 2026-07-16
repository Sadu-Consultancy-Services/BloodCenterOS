using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IRoleRepository
{
    Task<long> CreateAsync(long centerId, string name, string? desc, long createdBy);
    Task<IEnumerable<Role>> GetAllAsync(long? centerId);
    Task<IEnumerable<RolePermission>> GetPermissionsAsync(long roleId, long centerId);
    Task AssignPermissionAsync(long roleId, long permissionId, long centerId, long assignedBy);
    Task RemovePermissionAsync(long roleId, long permissionId, long centerId);
}
