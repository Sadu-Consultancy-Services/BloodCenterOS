using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly IDbConnectionFactory _db;
    public BranchRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Branch branch)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_branch_create(@p_center_id, @p_code, @p_name, @p_address_line1, @p_address_line2, @p_city, @p_state, @p_pincode, @p_phone, @p_email, @p_created_by)",
            new { p_center_id = branch.CenterId, p_code = branch.BranchCode, p_name = branch.BranchName, p_address_line1 = branch.AddressLine1, p_address_line2 = branch.AddressLine2, p_city = branch.City, p_state = branch.State, p_pincode = branch.Pincode, p_phone = branch.Phone, p_email = branch.Email, p_created_by = branch.CreatedBy });
    }

    public async Task UpdateAsync(Branch branch)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_branch_update(@p_branch_id, @p_code, @p_name, @p_address_line1, @p_address_line2, @p_city, @p_state, @p_pincode, @p_phone, @p_email)",
            new { p_branch_id = branch.BranchId, p_code = branch.BranchCode, p_name = branch.BranchName, p_address_line1 = branch.AddressLine1, p_address_line2 = branch.AddressLine2, p_city = branch.City, p_state = branch.State, p_pincode = branch.Pincode, p_phone = branch.Phone, p_email = branch.Email });
    }

    public async Task<Branch?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_branch_get_by_id(@p_branch_id)", new { p_branch_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<Branch>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_branch_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(b => b != null).Select(b => b!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_branch_delete(@p_branch_id)", new { p_branch_id = id });
    }

    private static Branch? Map(dynamic r)
    {
        if (r == null) return null;
        return new Branch
        {
            BranchId = (long)r.branchid,
            CenterId = (long?)r.centerid,
            BranchCode = (string?)r.branchcode,
            BranchName = (string?)r.branchname,
            AddressLine1 = (string?)r.addressline1,
            AddressLine2 = (string?)r.addressline2,
            City = (string?)r.city,
            State = (string?)r.state,
            Pincode = (string?)r.pincode,
            Phone = (string?)r.phone,
            Email = (string?)r.email,
            CreatedAt = (DateTime?)r.createdat,
            CreatedBy = (long?)r.createdby
        };
    }
}
