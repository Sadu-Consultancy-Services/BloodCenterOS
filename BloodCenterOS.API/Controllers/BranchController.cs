using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/branches")]
public class BranchController : ControllerBase
{
    private readonly IBranchRepository _repo;
    public BranchController(IBranchRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<Branch>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<Branch>.Fail("Branch not found"));
        return Ok(ApiResponse<Branch>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Branch branch)
    {
        branch.CenterId = CenterId;
        branch.CreatedBy = UserId;
        var id = await _repo.CreateAsync(branch);
        return Ok(ApiResponse<long>.Ok(id, "Branch created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Branch branch)
    {
        branch.BranchId = id;
        await _repo.UpdateAsync(branch);
        return Ok(ApiResponse<object>.Ok(new { }, "Branch updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Branch deleted"));
    }
}
