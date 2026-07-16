using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IHospitalRepository
{
    Task<long> CreateAsync(Hospital hospital);
    Task<IEnumerable<Hospital>> GetAllByCenterAsync(long centerId);
}
