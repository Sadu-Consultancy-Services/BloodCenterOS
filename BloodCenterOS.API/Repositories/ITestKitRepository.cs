using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ITestKitRepository
{
    Task<long> CreateAsync(long centerId, string name, string? manufacturer, string? lotNo, DateTime? expiry);
    Task<IEnumerable<TestKit>> GetAvailableAsync(long centerId);
}
