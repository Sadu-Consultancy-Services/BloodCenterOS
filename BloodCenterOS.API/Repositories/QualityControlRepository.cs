using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class QualityControlRepository : IQualityControlRepository
{
    private readonly IDbConnectionFactory _db;
    public QualityControlRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, long deviceId, string detail, long performedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_qc_create(@p_center_id, @p_device_id, @p_detail, @p_performed_by)",
            new { p_center_id = centerId, p_device_id = deviceId, p_detail = detail, p_performed_by = performedBy });
    }
}
