using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDbConnectionFactory _db;
    public AppointmentRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long donorId, DateTime date, string slot, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_appointment_create(@p_center_id, @p_donor_id, @p_date, @p_slot, @p_created_by)",
            new { p_center_id = centerId, p_donor_id = donorId, p_date = date, p_slot = slot, p_created_by = createdBy });
    }

    public async Task UpdateStatusAsync(long id, string status)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_appointment_update_status(@p_id, @p_status)", new { p_id = id, p_status = status });
    }
}
