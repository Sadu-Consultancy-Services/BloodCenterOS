using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class EmergencyRepository : IEmergencyRepository
{
    private readonly IDbConnectionFactory _db;

    public EmergencyRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateRequestAsync(EmergencyRequest request)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_emergency_request_create(@p_center_id, @p_hospital_id, @p_patient_name, @p_blood_group, @p_component_type, @p_units, @p_requested_by, @p_notes)",
            new
            {
                p_center_id = request.CenterId,
                p_hospital_id = request.HospitalId,
                p_patient_name = request.PatientName,
                p_blood_group = request.BloodGroup,
                p_component_type = request.ComponentType,
                p_units = request.UnitsRequired,
                p_requested_by = request.RequestedByUserId,
                p_notes = request.Notes
            });
    }

    public async Task<IEnumerable<EmergencyRequest>> GetPendingAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_emergency_request_get_pending(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new EmergencyRequest
        {
            EmergencyRequestId = (long)r.emergencyrequestid,
            CenterId = (long?)r.centerid,
            HospitalId = (long?)r.hospitalid,
            PatientName = (string?)r.patientname,
            BloodGroup = (string?)r.bloodgroup,
            ComponentType = (string?)r.componenttype,
            UnitsRequired = (int?)r.unitsrequired,
            RequestStatus = (string?)r.requeststatus,
            RequestedAt = (DateTime)r.requestedat,
            Notes = (string?)r.notes
        });
    }
}
