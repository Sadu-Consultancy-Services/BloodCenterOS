using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class CampRepository : ICampRepository
{
    private readonly IDbConnectionFactory _db;

    public CampRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(Camp camp, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_camp_create(@p_center_id, @p_camp_code, @p_camp_name, @p_organizer_id, @p_venue, @p_city, @p_camp_date, @p_start_time, @p_end_time, @p_expected, @p_created_by)",
            new
            {
                p_center_id = camp.CenterId,
                p_camp_code = camp.CampCode,
                p_camp_name = camp.CampName,
                p_organizer_id = camp.OrganizerId,
                p_venue = camp.Venue,
                p_city = camp.City,
                p_camp_date = camp.CampDate,
                p_start_time = camp.StartTime,
                p_end_time = camp.EndTime,
                p_expected = camp.TotalDonorsExpected,
                p_created_by = createdBy
            });
    }

    public async Task<Camp?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_camp_get_by_id(@p_camp_id)",
            new { p_camp_id = id });
        if (result == null) return null;
        return new Camp
        {
            CampId = (long)result.campid,
            CenterId = (long?)result.centerid,
            CampCode = (string?)result.campcode,
            CampName = (string?)result.campname,
            OrganizerId = (long?)result.organizerid,
            Venue = (string?)result.venue,
            City = (string?)result.city,
            CampDate = (DateTime?)result.campdate,
            StartTime = (TimeSpan?)result.starttime,
            EndTime = (TimeSpan?)result.endtime,
            TotalDonorsExpected = (int?)result.totaldonorsexpected,
            TotalDonorsCollected = (int?)result.totaldonorscollected,
            CreatedAt = (DateTime)result.createdat,
            CreatedBy = (long?)result.createdby
        };
    }

    public async Task<IEnumerable<Camp>> GetUpcomingAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_camp_get_upcoming(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new Camp
        {
            CampId = (long)r.campid,
            CampName = (string?)r.campname,
            Venue = (string?)r.venue,
            City = (string?)r.city,
            CampDate = (DateTime?)r.campdate,
            TotalDonorsExpected = (int?)r.totaldonorsexpected
        });
    }

    public async Task<IEnumerable<Camp>> GetByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_camp_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new Camp
        {
            CampId = (long)r.campid,
            CenterId = (long?)r.centerid,
            CampCode = (string?)r.campcode,
            CampName = (string?)r.campname,
            OrganizerId = (long?)r.organizerid,
            Venue = (string?)r.venue,
            City = (string?)r.city,
            CampDate = (DateTime?)r.campdate,
            StartTime = r.starttime == null ? (TimeSpan?)null : ((DateTime)r.starttime).TimeOfDay,
            EndTime = r.endtime == null ? (TimeSpan?)null : ((DateTime)r.endtime).TimeOfDay,
            TotalDonorsExpected = (int?)r.totaldonorsexpected,
            TotalDonorsCollected = (int?)r.totaldonorscollected,
            CreatedAt = (DateTime)r.createdat
        });
    }
}
