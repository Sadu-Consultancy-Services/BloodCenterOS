using BloodCenterOS.API.Data;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly IDbConnectionFactory _db;
    public ExpenseRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, string category, decimal amount, string? notes, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_expense_create(@p_center_id, @p_category, @p_amount, @p_notes, @p_created_by)",
            new { p_center_id = centerId, p_category = category, p_amount = amount, p_notes = notes, p_created_by = createdBy });
    }
}
