using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ILoginHistoryRepository
{
    Task<long> CreateAsync(long userId, long? centerId, string? ip, string? agent);
    Task LogoutAsync(long loginId);
}
