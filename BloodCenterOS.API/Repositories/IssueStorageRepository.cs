using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IIssueStorageRepository
{
    Task<IEnumerable<AvailableComponentForStorage>> GetAvailableComponentsAsync(long centerId);
    Task<decimal> GetStorageRateAsync(long storageId, string componentType);
    Task<long> CreateIssueAsync(long centerId, IssueToStorageRequest req, long userId);
    Task<IEnumerable<IssueStorageRecord>> GetByCenterAsync(long centerId, long? storageId, DateTime? from, DateTime? to);
    Task<IEnumerable<IssueStorageInvoice>> GetInvoicesAsync(long centerId, long? storageId, DateTime? from, DateTime? to);
}

public class IssueStorageRepository : IIssueStorageRepository
{
    private readonly string _conn;
    public IssueStorageRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<AvailableComponentForStorage>> GetAvailableComponentsAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<AvailableComponentForStorage>(
            "SELECT * FROM fn_issue_storage_get_available_components(@p_center_id)", new { p_center_id = centerId });
    }

    public async Task<decimal> GetStorageRateAsync(long storageId, string componentType)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<decimal>(
            "SELECT fn_issue_storage_get_storage_rate(@p_storage_id, @p_component_type)",
            new { p_storage_id = storageId, p_component_type = componentType });
    }

    public async Task<long> CreateIssueAsync(long centerId, IssueToStorageRequest req, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_issue_storage_create(@p_center_id, @p_storage_id, @p_component_ids, @p_issue_date::TIMESTAMPTZ, @p_payment_mode, @p_discount, @p_discount_reason, @p_em_amt, @p_notes, @p_created_by)",
            new
            {
                p_center_id = centerId, p_storage_id = req.StorageId,
                p_component_ids = req.ComponentIds, p_issue_date = req.IssueDate,
                p_payment_mode = req.PaymentMode, p_discount = req.Discount,
                p_discount_reason = req.DiscountReason, p_em_amt = req.EmAmt,
                p_notes = req.Notes, p_created_by = userId
            });
    }

    public async Task<IEnumerable<IssueStorageRecord>> GetByCenterAsync(long centerId, long? storageId, DateTime? from, DateTime? to)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<IssueStorageRecord>(
            "SELECT * FROM fn_issue_storage_get_by_center(@p_center_id, @p_storage_id, @p_from_date::DATE, @p_to_date::DATE)",
            new { p_center_id = centerId, p_storage_id = storageId, p_from_date = from, p_to_date = to });
    }

    public async Task<IEnumerable<IssueStorageInvoice>> GetInvoicesAsync(long centerId, long? storageId, DateTime? from, DateTime? to)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<IssueStorageInvoice>(
            "SELECT * FROM fn_issue_storage_get_invoices(@p_center_id, @p_storage_id, @p_from_date::DATE, @p_to_date::DATE)",
            new { p_center_id = centerId, p_storage_id = storageId, p_from_date = from, p_to_date = to });
    }
}
