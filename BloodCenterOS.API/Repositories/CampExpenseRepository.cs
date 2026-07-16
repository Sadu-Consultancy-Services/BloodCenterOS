using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class CampExpenseRepository : ICampExpenseRepository
{
    private readonly IDbConnectionFactory _db;
    public CampExpenseRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long campId, string category, decimal? amount, string? notes)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_camp_expense_create(@p_camp_id, @p_category, @p_amount, @p_notes)",
            new { p_camp_id = campId, p_category = category, p_amount = amount, p_notes = notes });
    }

    public async Task UpdateAsync(long id, string? category, decimal? amount, string? notes)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_camp_expense_update(@p_expense_id, @p_category, @p_amount, @p_notes)",
            new { p_expense_id = id, p_category = category, p_amount = amount, p_notes = notes });
    }

    public async Task<IEnumerable<CampExpense>> GetByCampAsync(long campId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_camp_expense_get_by_camp(@p_camp_id)", new { p_camp_id = campId });
        return rows.Select(MapExpense).Where(e => e != null).Select(e => e!);
    }

    public async Task<IEnumerable<CampExpense>> GetByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_camp_expense_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(MapExpenseWithCamp).Where(e => e != null).Select(e => e!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_camp_expense_delete(@p_expense_id)", new { p_expense_id = id });
    }

    private static CampExpense? MapExpense(dynamic r)
    {
        if (r == null) return null;
        return new CampExpense
        {
            CampExpenseId = (long)r.campexpenseid,
            CampId = (long)r.campid,
            ExpenseCategory = (string?)r.expensecategory,
            Amount = (decimal?)r.amount,
            Notes = (string?)r.notes,
            CreatedAt = (DateTime?)r.createdat
        };
    }

    private static CampExpense? MapExpenseWithCamp(dynamic r)
    {
        if (r == null) return null;
        return new CampExpense
        {
            CampExpenseId = (long)r.campexpenseid,
            CampId = (long)r.campid,
            CampName = (string?)r.campname,
            ExpenseCategory = (string?)r.expensecategory,
            Amount = (decimal?)r.amount,
            Notes = (string?)r.notes,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
