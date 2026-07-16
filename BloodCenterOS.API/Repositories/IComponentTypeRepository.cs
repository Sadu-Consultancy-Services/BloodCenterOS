using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IComponentTypeRepository
{
    Task<long> CreateAsync(string code, string? desc);
    Task UpdateAsync(long id, string? code, string? desc);
    Task<IEnumerable<ComponentType>> GetAllAsync();
    Task<ComponentType?> GetByIdAsync(long id);
    Task DeleteAsync(long id);
}
