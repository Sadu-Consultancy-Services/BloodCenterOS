using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _repo;
    public DepartmentController(IDepartmentRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<Department>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<Department>.Fail("Department not found"));
        return Ok(ApiResponse<Department>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Department department)
    {
        department.CenterId = CenterId;
        var id = await _repo.CreateAsync(department);
        return Ok(ApiResponse<long>.Ok(id, "Department created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Department department)
    {
        department.DepartmentId = id;
        await _repo.UpdateAsync(department);
        return Ok(ApiResponse<object>.Ok(new { }, "Department updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Department deleted"));
    }
}
