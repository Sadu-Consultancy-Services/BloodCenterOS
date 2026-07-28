using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class PatientRequestRepository : IPatientRequestRepository
{
    private readonly IDbConnectionFactory _db;
    public PatientRequestRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long? hospitalId, string patientName, int? age, string? gender,
        string bloodGroup, string componentType, int units, string urgency, long requestedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_patient_request_create(@p_center_id, @p_hospital_id, @p_patient_name, @p_age, @p_gender, @p_blood_group, @p_component_type, @p_units, @p_urgency, @p_requested_by)",
            new
            {
                p_center_id = centerId,
                p_hospital_id = hospitalId,
                p_patient_name = patientName,
                p_age = age,
                p_gender = gender,
                p_blood_group = bloodGroup,
                p_component_type = componentType,
                p_units = units,
                p_urgency = urgency,
                p_requested_by = requestedBy
            });
    }

    public async Task<IEnumerable<PatientRequest>> GetPendingAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PatientRequest>(
            "SELECT * FROM fn_patient_request_get_pending(@p_center_id)", new { p_center_id = centerId });
    }

    public async Task<IEnumerable<PatientRequest>> GetAllAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PatientRequest>(
            "SELECT * FROM fn_patient_request_get_all(@p_center_id)", new { p_center_id = centerId });
    }

    public async Task<PatientRequest?> GetByIdAsync(long centerId, long requestId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<PatientRequest>(
            "SELECT * FROM fn_patient_request_get_by_id(@p_center_id, @p_request_id)",
            new { p_center_id = centerId, p_request_id = requestId });
    }
}