using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IReplacementDonorRepository
{
    Task<long> RegisterAsync(long centerId, long requestId, long donorId);
    Task<IEnumerable<ReplacementDonor>> GetAllAsync(long centerId);
}
