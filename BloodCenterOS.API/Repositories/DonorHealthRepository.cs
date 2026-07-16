using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class DonorHealthRepository : IDonorHealthRepository
{
    private readonly IDbConnectionFactory _db;
    public DonorHealthRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long donorId, decimal? weight, decimal? temp, string? bp, decimal? hemoglobin, int? pulse, string? remarks, long recordedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_donor_health_create(@p_center_id, @p_donor_id, @p_weight, @p_temp, @p_bp, @p_hemoglobin, @p_pulse, @p_remarks, @p_recorded_by)",
            new { p_center_id = centerId, p_donor_id = donorId, p_weight = weight, p_temp = temp, p_bp = bp, p_hemoglobin = hemoglobin, p_pulse = pulse, p_remarks = remarks, p_recorded_by = recordedBy });
    }

    public async Task<IEnumerable<DonorHealth>> GetByDonorAsync(long donorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DonorHealth>(
            "SELECT donorhealthhistoryid, visitdate, weightkg, temperature, bloodpressure, hemoglobin, pulserate, remarks FROM fn_donor_health_get_by_donor(@p_donor_id)",
            new { p_donor_id = donorId });
    }
}
