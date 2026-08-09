using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IReservationRepository
{
    Task<long> CreateAsync(ReservationCreateRequest request, long centerId, long userId);
    Task<BloodRequest?> GetByIdAsync(long id);
    Task<IEnumerable<BloodRequest>> GetAllAsync(long centerId, string? status, DateTime? from, DateTime? to, string? keyword);
    Task<IEnumerable<BloodRequestDetail>> GetDetailsAsync(long requestId);
    Task<IEnumerable<AvailableComponentItem>> GetAvailableComponentsAsync(long centerId, string bloodGroup, string componentType, int units);
    Task<IEnumerable<BloodRequest>> GetPendingAsync(long centerId);
    Task CancelAsync(long requestId, string? reason);
}