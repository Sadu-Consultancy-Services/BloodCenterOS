using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
}
