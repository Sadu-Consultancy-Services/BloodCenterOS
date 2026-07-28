using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IRateRepository
{
    Task<long> UpsertAsync(RateUpsertRequest request, long centerId, long userId);
    Task<IEnumerable<RateMaster>> GetAllAsync(long centerId);
    Task<RateMaster?> GetByIdAsync(long id);
    Task DeleteAsync(long id);
}
