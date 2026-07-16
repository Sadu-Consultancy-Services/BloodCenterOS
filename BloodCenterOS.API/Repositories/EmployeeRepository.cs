using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDbConnectionFactory _db;
    public EmployeeRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Employee employee)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_employee_create(@p_center_id, @p_code, @p_first_name, @p_last_name, @p_email, @p_phone, @p_designation, @p_department_id, @p_join_date, @p_created_by)",
            new { p_center_id = employee.CenterId, p_code = employee.EmployeeCode, p_first_name = employee.FirstName, p_last_name = employee.LastName, p_email = employee.Email, p_phone = employee.Phone, p_designation = employee.Designation, p_department_id = employee.DepartmentId, p_join_date = employee.JoinDate?.ToDateTime(TimeOnly.MinValue), p_created_by = employee.CreatedBy });
    }

    public async Task UpdateAsync(Employee employee)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_employee_update(@p_employee_id, @p_code, @p_first_name, @p_last_name, @p_email, @p_phone, @p_designation, @p_department_id, @p_join_date)",
            new { p_employee_id = employee.EmployeeId, p_code = employee.EmployeeCode, p_first_name = employee.FirstName, p_last_name = employee.LastName, p_email = employee.Email, p_phone = employee.Phone, p_designation = employee.Designation, p_department_id = employee.DepartmentId, p_join_date = employee.JoinDate?.ToDateTime(TimeOnly.MinValue) });
    }

    public async Task<Employee?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_employee_get_by_id(@p_employee_id)", new { p_employee_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<Employee>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_employee_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(e => e != null).Select(e => e!);
    }

    public async Task ToggleActiveAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_employee_toggle_active(@p_employee_id)", new { p_employee_id = id });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_employee_delete(@p_employee_id)", new { p_employee_id = id });
    }

    private static Employee? Map(dynamic r)
    {
        if (r == null) return null;
        return new Employee
        {
            EmployeeId = (long)r.employeeid,
            CenterId = (long?)r.centerid,
            EmployeeCode = (string?)r.employeecode,
            FirstName = (string?)r.firstname,
            LastName = (string?)r.lastname,
            Email = (string?)r.email,
            Phone = (string?)r.phone,
            Designation = (string?)r.designation,
            DepartmentId = (long?)r.departmentid,
            JoinDate = r.joindate != null ? DateOnly.FromDateTime((DateTime)r.joindate) : null,
            IsActive = (bool)r.isactive,
            CreatedAt = (DateTime?)r.createdat,
            CreatedBy = (long?)r.createdby,
            UpdatedAt = (DateTime?)r.updatedat
        };
    }
}
