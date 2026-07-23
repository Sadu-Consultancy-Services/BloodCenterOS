using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class HospitalRepository : IHospitalRepository
{
    private readonly IDbConnectionFactory _db;

    public HospitalRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Hospital>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_hospital_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new Hospital
        {
            HospitalId = (long)r.hospitalid,
            CenterId = (long?)r.centerid,
            HospitalCode = (string?)r.hospitalcode,
            HospitalName = (string)r.hospitalname,
            Address = (string?)r.address,
            ContactPerson = (string?)r.contactperson,
            Phone = (string?)r.phone,
            Email = (string?)r.email,
            CreatedAt = (DateTime)r.createdat
        });
    }

    public async Task<Hospital?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var r = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_hospital_get_by_id(@p_hospital_id)",
            new { p_hospital_id = id });
        if (r == null) return null;
        return new Hospital
        {
            HospitalId = (long)r.hospitalid,
            CenterId = (long?)r.centerid,
            HospitalCode = (string?)r.hospitalcode,
            HospitalName = (string)r.hospitalname,
            Address = (string?)r.address,
            ContactPerson = (string?)r.contactperson,
            Phone = (string?)r.phone,
            Email = (string?)r.email,
            CreatedAt = (DateTime)r.createdat
        };
    }

    public async Task<long> CreateAsync(Hospital hospital)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_hospital_create(@p_center_id, @p_code, @p_name, @p_address, @p_contact, @p_phone, @p_email)",
            new
            {
                p_center_id = hospital.CenterId,
                p_code = hospital.HospitalCode,
                p_name = hospital.HospitalName,
                p_address = hospital.Address,
                p_contact = hospital.ContactPerson,
                p_phone = hospital.Phone,
                p_email = hospital.Email
            });
    }

    public async Task UpdateAsync(Hospital hospital)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_hospital_update(@p_hospital_id, @p_code, @p_name, @p_address, @p_contact, @p_phone, @p_email)",
            new
            {
                p_hospital_id = hospital.HospitalId,
                p_code = hospital.HospitalCode,
                p_name = hospital.HospitalName,
                p_address = hospital.Address,
                p_contact = hospital.ContactPerson,
                p_phone = hospital.Phone,
                p_email = hospital.Email
            });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_hospital_delete(@p_hospital_id)",
            new { p_hospital_id = id });
    }
}
