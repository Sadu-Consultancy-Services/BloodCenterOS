using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly IDbConnectionFactory _db;

    public IssueRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateIssueAsync(IssueRecord issue)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_issue_create(@p_center_id, @p_component_id, @p_bag_id, @p_patient_name, @p_hospital_id, @p_issued_by, @p_issue_type, @p_slip_no, @p_notes)",
            new
            {
                p_center_id = issue.CenterId,
                p_component_id = issue.ComponentId,
                p_bag_id = issue.BagId,
                p_patient_name = issue.PatientName,
                p_hospital_id = issue.HospitalId,
                p_issued_by = issue.IssuedByUserId,
                p_issue_type = issue.IssueType,
                p_slip_no = issue.IssueSlipNumber,
                p_notes = issue.Notes
            });
    }

    public async Task<IEnumerable<IssueRecord>> GetByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_issue_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new IssueRecord
        {
            IssueRecordId = (long)r.issuerecordid,
            CenterId = (long?)r.centerid,
            ComponentId = (long?)r.componentid,
            BagId = (long?)r.bagid,
            PatientName = (string?)r.patientname,
            HospitalId = (long?)r.hospitalid,
            IssueDate = (DateTime)r.issuedate,
            IssuedByUserId = (long?)r.issuedbyuserid,
            IssueType = (string?)r.issuetype,
            IssueSlipNumber = (string?)r.issueslipnumber,
            Notes = (string?)r.notes
        });
    }

    public async Task<IEnumerable<PatientRequest>> GetPendingRequestsAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_patient_request_get_pending(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new PatientRequest
        {
            RequestId = (long)r.requestid,
            PatientName = (string?)r.patientname,
            BloodGroup = (string?)r.bloodgroup,
            ComponentType = (string?)r.componenttype,
            UnitsRequested = (int?)r.unitsrequested,
            RequestUrgency = (string?)r.requesturgency,
            RequestDate = (DateTime)r.requestdate
        });
    }
}
