namespace BloodCenterOS.API.Repositories;

public interface IAppointmentRepository
{
    Task<long> CreateAsync(long centerId, long donorId, DateTime date, string slot, long createdBy);
    Task UpdateStatusAsync(long id, string status);
}
