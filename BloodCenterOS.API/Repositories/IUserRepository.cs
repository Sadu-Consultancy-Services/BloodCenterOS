using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IUserRepository
{
    Task<long> CreateAsync(User user);
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUserNameAsync(string userName);
    Task<IEnumerable<dynamic>> SearchAsync(long? centerId, string? keyword, int page, int size);
    Task UpdateAsync(long userId, string? displayName, string? email, string? phone, long updatedBy);
    Task UpdatePasswordAsync(long userId, string hash, string salt);
    Task ToggleLockAsync(long userId, bool locked);
    Task UpdateLoginAsync(long userId);
    Task<IEnumerable<Role>> GetRolesAsync(long userId);
    Task AssignRoleAsync(long userId, long roleId, long centerId, long assignedBy);
    Task RemoveRoleAsync(long userId, long roleId);
}
