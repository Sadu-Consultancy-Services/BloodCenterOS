using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IBloodBagRepository
{
    Task<BloodBag?> GetByNumberAsync(string bagNo);
    Task UpdateStatusAsync(long bagId, string status);
    Task<IEnumerable<BloodBag>> SearchAsync(long centerId, string? term);
}
