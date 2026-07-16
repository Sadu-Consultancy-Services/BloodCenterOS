using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IBranchRepository
{
    Task<long> CreateAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task<Branch?> GetByIdAsync(long id);
    Task<IEnumerable<Branch>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
