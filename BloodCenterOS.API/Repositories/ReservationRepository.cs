using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly IDbConnectionFactory _db;
    public ReservationRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(ReservationCreateRequest request, long centerId, long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_reservation_create(@p_center_id, @p_patient_name, @p_patient_address, " +
            "@p_patient_contact_no, @p_patient_blood_group, @p_required_blood_group, " +
            "@p_hospital_name, @p_ward, @p_component_type, @p_units, @p_create_invoice, @p_created_by, @p_notes)",
            new
            {
                p_center_id = centerId,
                p_patient_name = request.PatientName,
                p_patient_address = request.PatientAddress,
                p_patient_contact_no = request.PatientContactNo,
                p_patient_blood_group = request.PatientBloodGroup,
                p_required_blood_group = request.RequiredBloodGroup,
                p_hospital_name = request.HospitalName,
                p_ward = request.Ward,
                p_component_type = request.ComponentType,
                p_units = request.Units,
                p_create_invoice = request.CreateInvoice,
                p_created_by = userId,
                p_notes = request.Notes
            });
    }

    public async Task<BloodRequest?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var r = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_reservation_get_by_id(@p_reservation_id)", new { p_reservation_id = id });
        return r == null ? null : MapReservation(r);
    }

    public async Task<IEnumerable<BloodRequest>> GetAllAsync(long centerId, string? status, DateTime? from, DateTime? to, string? keyword)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_reservation_get_by_center(@p_center_id, @p_status, @p_from_date, @p_to_date, @p_keyword)",
            new { p_center_id = centerId, p_status = status, p_from_date = from, p_to_date = to, p_keyword = keyword });
        return rows.Select(r => (BloodRequest?)MapReservation(r)).Where(x => x != null).Cast<BloodRequest>();
    }

    public async Task<IEnumerable<BloodRequestDetail>> GetDetailsAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_reservation_get_details(@p_reservation_id)", new { p_reservation_id = requestId });
        return rows.Select(r => (BloodRequestDetail?)MapDetail(r)).Where(x => x != null).Cast<BloodRequestDetail>();
    }

    public async Task<IEnumerable<AvailableComponentItem>> GetAvailableComponentsAsync(long centerId, string bloodGroup, string componentType, int units)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_reservation_get_available_components(@p_center_id, @p_blood_group, @p_component_type, @p_units)",
            new { p_center_id = centerId, p_blood_group = bloodGroup, p_component_type = componentType, p_units = units });
        return rows.Select(r => (AvailableComponentItem?)MapAvailable(r)).Where(x => x != null).Cast<AvailableComponentItem>();
    }

    public async Task<IEnumerable<BloodRequest>> GetPendingAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_reservation_get_pending(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(r => (BloodRequest?)MapSimple(r)).Where(x => x != null).Cast<BloodRequest>();
    }

    public async Task CancelAsync(long requestId, string? reason)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_reservation_cancel(@p_reservation_id, @p_reason)",
            new { p_reservation_id = requestId, p_reason = reason });
    }

    private static BloodRequest MapReservation(dynamic r) => new()
    {
        BloodRequestId = (long)r.bloodrequestid,
        CenterId = (long)r.centerid,
        PatientName = (string)r.patientname,
        PatientAddress = (string?)r.patientaddress,
        PatientContactNo = (string?)r.patientcontactno,
        PatientBloodGroup = (string)r.patientbloodgroup,
        RequiredBloodGroup = (string)r.requiredbloodgroup,
        HospitalName = (string?)r.hospitalname,
        Ward = (string?)r.ward,
        ComponentType = (string)r.componenttype,
        UnitsRequested = (int)r.unitsrequested,
        UnitsReserved = (int)r.unitsreserved,
        Status = (string)r.status,
        ReservationDate = (DateTime)r.reservationdate,
        InvoiceId = (long?)r.invoiceid,
        Notes = (string?)r.notes,
        CreatedAt = (DateTime)r.createdat,
        CreatedBy = (long?)r.createdby
    };

    private static BloodRequest MapSimple(dynamic r) => new()
    {
        BloodRequestId = (long)r.bloodrequestid,
        PatientName = (string)r.patientname,
        PatientBloodGroup = (string)r.patientbloodgroup,
        RequiredBloodGroup = (string)r.requiredbloodgroup,
        HospitalName = (string?)r.hospitalname,
        ComponentType = (string)r.componenttype,
        UnitsReserved = (int)r.unitsreserved,
        ReservationDate = (DateTime)r.reservationdate,
        Status = "Active"
    };

    private static BloodRequestDetail MapDetail(dynamic r) => new()
    {
        BloodRequestDetailId = (long)r.bloodrequestdetailid,
        BloodRequestId = (long)r.bloodrequestid,
        ComponentId = (long)r.componentid,
        ComponentCode = (string?)r.componentcode,
        BloodGroup = (string?)r.bloodgroup,
        ComponentType = (string?)r.componenttype,
        VolumeMl = (int?)r.volumeml,
        ExpiryDate = (DateTime?)r.expirydate,
        UnitRate = (decimal)r.unitrate,
        ReservationRate = (decimal)r.reservationrate,
        Status = (string?)r.status,
        CreatedAt = (DateTime)r.createdat
    };

    private static AvailableComponentItem MapAvailable(dynamic r) => new()
    {
        ComponentId = (long)r.componentid,
        ComponentCode = (string)r.componentcode,
        ComponentType = (string?)r.componenttype,
        VolumeMl = (int?)r.volumeml,
        BloodGroup = (string?)r.bloodgroup,
        ExpiryDate = (DateTime?)r.expirydate,
        StorageLocation = (string?)r.storagelocation,
        UnitRate = (decimal)r.rate,
        ReservationRate = (decimal)r.reservationrate
    };
}
