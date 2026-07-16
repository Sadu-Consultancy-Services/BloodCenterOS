using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class TestKitRepository : ITestKitRepository
{
    private readonly IDbConnectionFactory _db;
    public TestKitRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, string name, string? manufacturer, string? lotNo, DateTime? expiry)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_test_kit_create(@p_center_id, @p_name, @p_manufacturer, @p_lot_no, @p_expiry)",
            new { p_center_id = centerId, p_name = name, p_manufacturer = manufacturer, p_lot_no = lotNo, p_expiry = expiry });
    }

    public async Task<IEnumerable<TestKit>> GetAvailableAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<TestKit>(
            "SELECT testkitid, kitname, lotnumber, expirydate FROM fn_test_kit_get_available(@p_center_id)",
            new { p_center_id = centerId });
    }
}
