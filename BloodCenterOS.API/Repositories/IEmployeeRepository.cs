using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IEmployeeRepository
{
    Task<long> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task<Employee?> GetByIdAsync(long id);
    Task<IEnumerable<Employee>> GetAllByCenterAsync(long centerId);
    Task ToggleActiveAsync(long id);
    Task DeleteAsync(long id);
}
