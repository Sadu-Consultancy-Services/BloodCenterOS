using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class DeferralRepository : IDeferralRepository
{
    private readonly IDbConnectionFactory _db;
    public DeferralRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long donorId, string reason, DateTime? until, string? notes, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_deferral_create(@p_center_id, @p_donor_id, @p_reason, @p_until, @p_notes, @p_created_by)",
            new { p_center_id = centerId, p_donor_id = donorId, p_reason = reason, p_until = until, p_notes = notes, p_created_by = createdBy });
    }

    public async Task<IEnumerable<DeferralRecord>> GetActiveAsync(long donorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DeferralRecord>(
            "SELECT deferralid, deferraldate, reason, deferraluntil FROM fn_deferral_get_active(@p_donor_id)",
            new { p_donor_id = donorId });
    }
}
