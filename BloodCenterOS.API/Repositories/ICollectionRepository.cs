using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ICollectionRepository
{
    Task<long> CreateAsync(Collection collection, long createdBy);
    Task<Collection?> GetByIdAsync(long id);
    Task<IEnumerable<Collection>> GetByCenterAsync(long centerId);
}
