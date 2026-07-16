using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ICampRepository
{
    Task<long> CreateAsync(Camp camp, long createdBy);
    Task<Camp?> GetByIdAsync(long id);
    Task<IEnumerable<Camp>> GetUpcomingAsync(long centerId);
}
