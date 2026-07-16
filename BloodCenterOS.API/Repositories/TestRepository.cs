using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class TestRepository : ITestRepository
{
    private readonly IDbConnectionFactory _db;

    public TestRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateRecordAsync(long centerId, long? collectionId, string? bagNumber, long performedBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_test_record_create(@p_center_id, @p_collection_id, @p_bag_no, @p_performed_by)",
            new { p_center_id = centerId, p_collection_id = collectionId, p_bag_no = bagNumber, p_performed_by = performedBy });
    }

    public async Task<BloodTestRecord?> GetRecordByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var r = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM BloodTestRecord WHERE TestRecordId = @id", new { id });
        if (r == null) return null;
        return new BloodTestRecord
        {
            TestRecordId = (long)r.testrecordid,
            CenterId = (long?)r.centerid,
            CollectionId = (long?)r.collectionid,
            BagNumber = (string?)r.bagnumber,
            SampleTakenAt = (DateTime?)r.sampletakenat,
            PerformedBy = (long?)r.performedby,
            OverallStatus = (string?)r.overallstatus,
            CreatedAt = (DateTime)r.createdat
        };
    }

    public async Task<IEnumerable<BloodTestRecord>> GetPendingAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM BloodTestRecord WHERE CenterId = @cid AND OverallStatus = 'Pending' ORDER BY CreatedAt DESC",
            new { cid = centerId });
        return rows.Select(r => new BloodTestRecord
        {
            TestRecordId = (long)r.testrecordid,
            CenterId = (long?)r.centerid,
            CollectionId = (long?)r.collectionid,
            BagNumber = (string?)r.bagnumber,
            SampleTakenAt = (DateTime?)r.sampletakenat,
            PerformedBy = (long?)r.performedby,
            OverallStatus = (string?)r.overallstatus,
            CreatedAt = (DateTime)r.createdat
        });
    }

    public async Task<long> AddResultAsync(BloodTestResult result)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_test_result_add(@p_center_id, @p_test_record_id, @p_bag_id, @p_test_code, @p_result, @p_method, @p_kit_lot, @p_performed_by, @p_remarks)",
            new
            {
                p_center_id = result.CenterId,
                p_test_record_id = result.TestRecordId,
                p_bag_id = result.BagId,
                p_test_code = result.TestCode,
                p_result = result.Result,
                p_method = result.Method,
                p_kit_lot = result.KitLotNo,
                p_performed_by = result.PerformedBy,
                p_remarks = result.Remarks
            });
    }

    public async Task<IEnumerable<BloodTestResult>> GetResultsByRecordAsync(long testRecordId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_test_result_get_by_record(@p_test_record_id)",
            new { p_test_record_id = testRecordId });
        return rows.Select(r => new BloodTestResult
        {
            TestResultId = (long)r.testresultid,
            CenterId = (long?)r.centerid,
            TestRecordId = (long?)r.testrecordid,
            BagId = (long?)r.bagid,
            TestCode = (string)r.testcode,
            Result = (string?)r.result,
            Method = (string?)r.method,
            KitLotNo = (string?)r.kitlotno,
            PerformedBy = (long?)r.performedby,
            PerformedAt = (DateTime?)r.performedat,
            Remarks = (string?)r.remarks
        });
    }

    public async Task UpdateRecordStatusAsync(long recordId, string status)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_test_record_update_status(@p_record_id, @p_status)",
            new { p_record_id = recordId, p_status = status });
    }
}
