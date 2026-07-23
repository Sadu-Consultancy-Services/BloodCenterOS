using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class CampOrganizerRepository : ICampOrganizerRepository
{
    private readonly IDbConnectionFactory _db;
    public CampOrganizerRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(CampOrganizer organizer)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_camp_organizer_create(@p_center_id, @p_name, @p_contact, @p_phone, @p_email, @p_address)",
            new { p_center_id = organizer.CenterId, p_name = organizer.OrganizerName, p_contact = organizer.ContactPerson, p_phone = organizer.Phone, p_email = organizer.Email, p_address = organizer.Address });
    }

    public async Task UpdateAsync(CampOrganizer organizer)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_camp_organizer_update(@p_organizer_id, @p_name, @p_contact, @p_phone, @p_email, @p_address)",
            new { p_organizer_id = organizer.OrganizerId, p_name = organizer.OrganizerName, p_contact = organizer.ContactPerson, p_phone = organizer.Phone, p_email = organizer.Email, p_address = organizer.Address });
    }

    public async Task<CampOrganizer?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var r = await conn.QueryFirstOrDefaultAsync<dynamic>("SELECT * FROM fn_camp_organizer_get_by_id(@p_organizer_id)", new { p_organizer_id = id });
        return r == null ? null : Map(r);
    }

    public async Task<IEnumerable<CampOrganizer>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_camp_organizer_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(x => x != null).Select(x => x!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_camp_organizer_delete(@p_organizer_id)", new { p_organizer_id = id });
    }

    private static CampOrganizer Map(dynamic r) => new()
    {
        OrganizerId = (long)r.organizerid,
        CenterId = (long)r.centerid,
        OrganizerName = (string?)r.organizername,
        ContactPerson = (string?)r.contactperson,
        Phone = (string?)r.phone,
        Email = (string?)r.email,
        Address = (string?)r.address,
        CreatedAt = (DateTime)r.createdat
    };
}
