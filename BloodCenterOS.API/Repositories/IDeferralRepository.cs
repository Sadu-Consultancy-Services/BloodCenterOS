using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IDeferralRepository
{
    Task<long> CreateAsync(long centerId, long donorId, string reason, DateTime? until, string? notes, long createdBy);
    Task<IEnumerable<DeferralRecord>> GetActiveAsync(long donorId);
}
