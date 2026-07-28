using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IPatientRequestRepository
{
    Task<long> CreateAsync(long centerId, long? hospitalId, string patientName, int? age, string? gender,
        string bloodGroup, string componentType, int units, string urgency, long requestedBy);
    Task<IEnumerable<PatientRequest>> GetPendingAsync(long centerId);
    Task<IEnumerable<PatientRequest>> GetAllAsync(long centerId);
    Task<PatientRequest?> GetByIdAsync(long centerId, long requestId);
}
