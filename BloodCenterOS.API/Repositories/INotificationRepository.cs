using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface INotificationRepository
{
    Task<long> CreateAsync(long centerId, string type, string title, string body, string audience);
    Task<IEnumerable<Notification>> GetAllAsync(long centerId);
}
