using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly IDbConnectionFactory _db;
    public DepartmentRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Department department)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_department_create(@p_center_id, @p_code, @p_name, @p_description)",
            new { p_center_id = department.CenterId, p_code = department.DepartmentCode, p_name = department.DepartmentName, p_description = department.Description });
    }

    public async Task UpdateAsync(Department department)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_department_update(@p_department_id, @p_code, @p_name, @p_description)",
            new { p_department_id = department.DepartmentId, p_code = department.DepartmentCode, p_name = department.DepartmentName, p_description = department.Description });
    }

    public async Task<Department?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_department_get_by_id(@p_department_id)", new { p_department_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<Department>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_department_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(d => d != null).Select(d => d!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_department_delete(@p_department_id)", new { p_department_id = id });
    }

    private static Department? Map(dynamic r)
    {
        if (r == null) return null;
        return new Department
        {
            DepartmentId = (long)r.departmentid,
            CenterId = (long?)r.centerid,
            DepartmentCode = (string?)r.departmentcode,
            DepartmentName = (string)r.departmentname,
            Description = (string?)r.description,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
