using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IIssueRepository
{
    Task<long> IssueFromReservationAsync(long centerId, long reservationId, string? paymentMode, long? userId, string? notes);
    Task<IEnumerable<IssueRecord>> GetByCenterAsync(long centerId);
    Task<IEnumerable<IssueRecord>> GetByReservationAsync(long reservationId);
    Task<IEnumerable<ReservationReadyForIssue>> GetReadyForIssueAsync(long centerId);
}

public class IssueRepository : IIssueRepository
{
    private readonly string _conn;
    public IssueRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<long> IssueFromReservationAsync(long centerId, long reservationId, string? paymentMode, long? userId, string? notes)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_issue_from_reservation(@p_center_id, @p_reservation_id, 'Patient', @p_payment_mode, @p_issued_by, @p_notes)",
            new { p_center_id = centerId, p_reservation_id = reservationId, p_payment_mode = paymentMode, p_issued_by = userId, p_notes = notes });
    }

    public async Task<IEnumerable<IssueRecord>> GetByCenterAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<IssueRecord>(
            "SELECT i.*, cm.componentcode, cm.componenttype FROM IssueRecord i " +
            "JOIN ComponentMaster cm ON cm.componentid = i.ComponentId " +
            "WHERE i.CenterId = @cid ORDER BY i.IssueDate DESC",
            new { cid = centerId });
    }

    public async Task<IEnumerable<IssueRecord>> GetByReservationAsync(long reservationId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<IssueRecord>(
            "SELECT * FROM fn_issue_get_by_reservation(@p_reservation_id)",
            new { p_reservation_id = reservationId });
    }

    public async Task<IEnumerable<ReservationReadyForIssue>> GetReadyForIssueAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<ReservationReadyForIssue>(
            "SELECT * FROM fn_issue_get_ready_for_issue(@p_center_id)",
            new { p_center_id = centerId });
    }
}
