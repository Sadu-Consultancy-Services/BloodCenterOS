using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IReservationRepository
{
    Task<long> CreateAsync(ReservationCreateRequest request, long centerId, long userId);
    Task<PatientReservation?> GetByIdAsync(long id);
    Task<IEnumerable<PatientReservation>> GetAllAsync(long centerId, string? status, DateTime? from, DateTime? to, string? keyword);
    Task<IEnumerable<ReservationDetail>> GetDetailsAsync(long reservationId);
    Task<IEnumerable<AvailableComponentItem>> GetAvailableComponentsAsync(long centerId, string bloodGroup, string componentType, int units);
    Task<IEnumerable<PatientReservation>> GetPendingAsync(long centerId);
    Task CancelAsync(long reservationId, string? reason);
}
