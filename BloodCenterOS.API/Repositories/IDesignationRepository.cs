using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IDesignationRepository
{
    Task<long> CreateAsync(Designation designation);
    Task UpdateAsync(Designation designation);
    Task<Designation?> GetByIdAsync(long id);
    Task<IEnumerable<Designation>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
