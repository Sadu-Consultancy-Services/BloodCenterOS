using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ReplacementDonorRepository : IReplacementDonorRepository
{
    private readonly IDbConnectionFactory _db;
    public ReplacementDonorRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> RegisterAsync(long centerId, long requestId, long donorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_replacement_donor_register(@p_center_id, @p_request_id, @p_donor_id)",
            new { p_center_id = centerId, p_request_id = requestId, p_donor_id = donorId });
    }
}
