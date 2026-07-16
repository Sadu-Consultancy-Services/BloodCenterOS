using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/designations")]
public class DesignationController : ControllerBase
{
    private readonly IDesignationRepository _repo;
    public DesignationController(IDesignationRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<Designation>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<Designation>.Fail("Designation not found"));
        return Ok(ApiResponse<Designation>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Designation designation)
    {
        designation.CenterId = CenterId;
        var id = await _repo.CreateAsync(designation);
        return Ok(ApiResponse<long>.Ok(id, "Designation created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Designation designation)
    {
        designation.DesignationId = id;
        await _repo.UpdateAsync(designation);
        return Ok(ApiResponse<object>.Ok(new { }, "Designation updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Designation deleted"));
    }
}
