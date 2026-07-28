using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface ICrossMatchRepository
{
    Task<long> StartAsync(long centerId, long reservationId, long userId);
    Task SetTestResultAsync(long testResultId, string result);
    Task RejectComponentAsync(long testResultId);
    Task<IEnumerable<CrossMatchEntry>> GetPendingReservationsAsync(long centerId);
    Task<IEnumerable<CrossMatchEntry>> GetByCenterAsync(long centerId, string? status, DateTime? from, DateTime? to);
    Task<CrossMatchEntry?> GetByIdAsync(long entryId);
    Task<IEnumerable<CrossMatchTestResult>> GetTestsAsync(long entryId);
}

public class CrossMatchRepository : ICrossMatchRepository
{
    private readonly string _conn;
    public CrossMatchRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<long> StartAsync(long centerId, long reservationId, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_crossmatch_start(@p_center_id, @p_reservation_id, @p_performed_by)",
            new { p_center_id = centerId, p_reservation_id = reservationId, p_performed_by = userId });
    }

    public async Task SetTestResultAsync(long testResultId, string result)
    {
        using var db = new NpgsqlConnection(_conn);
        await db.ExecuteAsync("SELECT fn_crossmatch_set_result(@p_test_result_id, @p_result)",
            new { p_test_result_id = testResultId, p_result = result });
    }

    public async Task RejectComponentAsync(long testResultId)
    {
        using var db = new NpgsqlConnection(_conn);
        await db.ExecuteAsync("SELECT fn_crossmatch_reject_component(@p_test_result_id)",
            new { p_test_result_id = testResultId });
    }

    public async Task<IEnumerable<CrossMatchEntry>> GetPendingReservationsAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<CrossMatchEntry>(
            "SELECT * FROM fn_crossmatch_get_pending_reservations(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task<IEnumerable<CrossMatchEntry>> GetByCenterAsync(long centerId, string? status, DateTime? from, DateTime? to)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<CrossMatchEntry>(
            "SELECT * FROM fn_crossmatch_get_by_center(@p_center_id, @p_status, @p_from_date::DATE, @p_to_date::DATE)",
            new { p_center_id = centerId, p_status = status, p_from_date = from, p_to_date = to });
    }

    public async Task<CrossMatchEntry?> GetByIdAsync(long entryId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryFirstOrDefaultAsync<CrossMatchEntry>(
            "SELECT * FROM fn_crossmatch_get_by_id(@p_entry_id)",
            new { p_entry_id = entryId });
    }

    public async Task<IEnumerable<CrossMatchTestResult>> GetTestsAsync(long entryId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<CrossMatchTestResult>(
            "SELECT * FROM fn_crossmatch_get_tests(@p_entry_id)",
            new { p_entry_id = entryId });
    }
}
