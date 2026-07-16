using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IEmergencyRepository
{
    Task<long> CreateRequestAsync(EmergencyRequest request);
    Task<IEnumerable<EmergencyRequest>> GetPendingAsync(long centerId);
}
