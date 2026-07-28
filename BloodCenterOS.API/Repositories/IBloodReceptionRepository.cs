using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IBloodReceptionRepository
{
    Task<long> CreateAsync(BloodReceptionCreateRequest request, long centerId);
    Task<BloodReception?> GetByIdAsync(long id);
    Task<IEnumerable<BloodReception>> GetAllByCenterAsync(long centerId, DateTime? from, DateTime? to);
    Task<IEnumerable<BloodReceptionDetail>> GetDetailsAsync(long receptionId);
}
