using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly IDepartmentRepository _deptRepo;
    public EmployeeController(IEmployeeRepository repo, IDepartmentRepository deptRepo)
    {
        _repo = repo;
        _deptRepo = deptRepo;
    }

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        var depts = (await _deptRepo.GetAllByCenterAsync(CenterId)).ToDictionary(d => d.DepartmentId);
        foreach (var e in data)
            if (e.DepartmentId.HasValue && depts.TryGetValue(e.DepartmentId.Value, out var d))
                e.DepartmentName = d.DepartmentName;
        return Ok(ApiResponse<IEnumerable<Employee>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<Employee>.Fail("Employee not found"));
        return Ok(ApiResponse<Employee>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Employee employee)
    {
        employee.CenterId = CenterId;
        employee.CreatedBy = UserId;
        var id = await _repo.CreateAsync(employee);
        return Ok(ApiResponse<long>.Ok(id, "Employee created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Employee employee)
    {
        employee.EmployeeId = id;
        await _repo.UpdateAsync(employee);
        return Ok(ApiResponse<object>.Ok(new { }, "Employee updated"));
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(long id)
    {
        await _repo.ToggleActiveAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Status toggled"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Employee deleted"));
    }
}
