using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ILoginHistoryRepository
{
    Task<long> CreateAsync(long userId, long? centerId, string? ip, string? agent);
    Task LogoutAsync(long loginId);
    Task<IEnumerable<LoginHistory>> GetFilteredAsync(long? userId, DateTime? fromDate, DateTime? toDate, int limit = 200);
}
