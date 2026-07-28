using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;
using System.Text.Json;

namespace BloodCenterOS.API.Repositories;

public class BloodReceptionRepository : IBloodReceptionRepository
{
    private readonly IDbConnectionFactory _db;
    public BloodReceptionRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(BloodReceptionCreateRequest request, long centerId)
    {
        using var conn = _db.CreateConnection();
        var detailsJson = JsonSerializer.Serialize(request.Details.Select(d => new
        {
            donorName = d.DonorName,
            sex = d.Sex,
            bloodGroup = d.BloodGroup,
            contactNo = d.ContactNo,
            bagNumber = d.BagNumber,
            bagType = d.BagType,
            expiryDate = d.ExpiryDate?.ToString("yyyy-MM-dd"),
            volumeMl = d.VolumeMl
        }), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_blood_reception_create(@p_center_id, @p_mbb_name, @p_receipt_date, @p_bill_number, @p_notes, @p_received_by, @p_details::JSONB)",
            new
            {
                p_center_id = centerId,
                p_mbb_name = request.MBBName,
                p_receipt_date = request.ReceiptDate,
                p_bill_number = request.BillNumber,
                p_notes = request.Notes,
                p_received_by = request.ReceivedBy,
                p_details = detailsJson
            });
    }

    public async Task<BloodReception?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var r = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_blood_reception_get_by_id(@p_reception_id)", new { p_reception_id = id });
        return r == null ? null : MapReception(r);
    }

    public async Task<IEnumerable<BloodReception>> GetAllByCenterAsync(long centerId, DateTime? from, DateTime? to)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_blood_reception_get_by_center(@p_center_id, @p_from_date, @p_to_date)",
            new { p_center_id = centerId, p_from_date = from, p_to_date = to });
        var receptions = rows.Select(r => (BloodReception?)MapReception(r)!).Where(x => x != null).Cast<BloodReception>().ToList();

        foreach (var rec in receptions)
        {
            var details = await GetDetailsAsync(rec.ReceptionId);
            rec.Details = details.ToList();
        }
        return receptions;
    }

    public async Task<IEnumerable<BloodReceptionDetail>> GetDetailsAsync(long receptionId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_blood_reception_get_details(@p_reception_id)", new { p_reception_id = receptionId });
        return rows.Select(r => (BloodReceptionDetail?)MapDetail(r)).Where(x => x != null).Cast<BloodReceptionDetail>();
    }

    private static BloodReception? MapReception(dynamic r) => new()
    {
        ReceptionId = (long)r.receptionid,
        CenterId = (long)r.centerid,
        MBBName = (string)r.mbbname,
        ReceiptDate = (DateTime)r.receiptdate,
        BillNumber = (string?)r.billnumber,
        TotalBags = (int)r.totalbags,
        Notes = (string?)r.notes,
        ReceivedBy = (long?)r.receivedby,
        CreatedAt = (DateTime)r.createdat
    };

    private static BloodReceptionDetail? MapDetail(dynamic r) => new()
    {
        ReceptionDetailId = (long)r.receptiondetailid,
        ReceptionId = (long)r.receptionid,
        DonorName = (string)r.donorname,
        Sex = (string?)r.sex,
        BloodGroup = (string)r.bloodgroup,
        ContactNo = (string?)r.contactno,
        BagNumber = (string)r.bagnumber,
        BagType = (string)r.bagtype,
        ExpiryDate = (DateTime?)r.expirydate,
        VolumeMl = (int)r.volumeml,
        CreatedAt = (DateTime)r.createdat
    };
}
