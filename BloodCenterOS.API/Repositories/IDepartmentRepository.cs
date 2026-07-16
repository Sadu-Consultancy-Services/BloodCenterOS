using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IDepartmentRepository
{
    Task<long> CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task<Department?> GetByIdAsync(long id);
    Task<IEnumerable<Department>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
