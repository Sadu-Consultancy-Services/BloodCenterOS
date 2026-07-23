using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IHospitalRepository
{
    Task<Hospital?> GetByIdAsync(long id);
    Task<IEnumerable<Hospital>> GetAllByCenterAsync(long centerId);
    Task<long> CreateAsync(Hospital hospital);
    Task UpdateAsync(Hospital hospital);
    Task DeleteAsync(long id);
}
