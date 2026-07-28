using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IStoreInventoryRepository
{
    Task<IEnumerable<InvItem>> GetItemsAsync(long centerId);
    Task<IEnumerable<InvItem>> GetActiveItemsAsync(long centerId);
    Task<InvItem?> GetItemByIdAsync(long id);
    Task<long> UpsertItemAsync(long centerId, InvItem item, long userId);
    Task DeleteItemAsync(long id);
    Task<long> InwardAsync(long centerId, long itemId, int qty, string? desc, long userId);
    Task<long> OutwardAsync(long centerId, long itemId, int qty, string? desc, long userId);
    Task<IEnumerable<InvTransaction>> GetTransactionsAsync(long centerId, long itemId, DateTime? from, DateTime? to);
    Task<IEnumerable<InvStockSummary>> GetSummaryAsync(long centerId);
}

public class StoreInventoryRepository : IStoreInventoryRepository
{
    private readonly string _conn;
    public StoreInventoryRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<InvItem>> GetItemsAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<InvItem>("SELECT * FROM fn_inv_items_get_by_center(@p_center_id)", new { p_center_id = centerId });
    }

    public async Task<IEnumerable<InvItem>> GetActiveItemsAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<InvItem>("SELECT * FROM fn_inv_items_get_active(@p_center_id)", new { p_center_id = centerId });
    }

    public async Task<InvItem?> GetItemByIdAsync(long id)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryFirstOrDefaultAsync<InvItem>("SELECT * FROM fn_inv_items_get_by_id(@p_id)", new { p_id = id });
    }

    public async Task<long> UpsertItemAsync(long centerId, InvItem item, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_inv_items_upsert(@p_center_id, @p_id, @p_name, @p_min_qty, @p_unit, @p_is_active, @p_created_by)",
            new { p_center_id = centerId, p_id = item.ItemId, p_name = item.ItemName, p_min_qty = item.MinOrderQty, p_unit = item.ItemUnit, p_is_active = item.IsActive, p_created_by = userId });
    }

    public async Task DeleteItemAsync(long id)
    {
        using var db = new NpgsqlConnection(_conn);
        await db.ExecuteAsync("SELECT fn_inv_items_delete(@p_id)", new { p_id = id });
    }

    public async Task<long> InwardAsync(long centerId, long itemId, int qty, string? desc, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_inv_trans_inward(@p_center_id, @p_item_id, @p_qty, @p_desc, @p_created_by)",
            new { p_center_id = centerId, p_item_id = itemId, p_qty = qty, p_desc = desc, p_created_by = userId });
    }

    public async Task<long> OutwardAsync(long centerId, long itemId, int qty, string? desc, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_inv_trans_outward(@p_center_id, @p_item_id, @p_qty, @p_desc, @p_created_by)",
            new { p_center_id = centerId, p_item_id = itemId, p_qty = qty, p_desc = desc, p_created_by = userId });
    }

    public async Task<IEnumerable<InvTransaction>> GetTransactionsAsync(long centerId, long itemId, DateTime? from, DateTime? to)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<InvTransaction>(
            "SELECT * FROM fn_inv_trans_get_by_item(@p_center_id, @p_item_id, @p_from_date::DATE, @p_to_date::DATE)",
            new { p_center_id = centerId, p_item_id = itemId, p_from_date = from, p_to_date = to });
    }

    public async Task<IEnumerable<InvStockSummary>> GetSummaryAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<InvStockSummary>("SELECT * FROM fn_inv_trans_get_summary(@p_center_id)", new { p_center_id = centerId });
    }
}
