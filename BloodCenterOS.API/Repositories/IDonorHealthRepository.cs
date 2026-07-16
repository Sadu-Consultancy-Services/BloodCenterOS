using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IDonorHealthRepository
{
    Task<long> CreateAsync(long centerId, long donorId, decimal? weight, decimal? temp, string? bp, decimal? hemoglobin, int? pulse, string? remarks, long recordedBy);
    Task<IEnumerable<DonorHealth>> GetByDonorAsync(long donorId);
}
