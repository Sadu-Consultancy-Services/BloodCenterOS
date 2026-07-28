using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/rates")]
public class RateController : ControllerBase
{
    private readonly IRateRepository _repo;
    public RateController(IRateRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<RateMaster>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<RateMaster>.Fail("Rate not found"));
        return Ok(ApiResponse<RateMaster>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] RateUpsertRequest request)
    {
        var id = await _repo.UpsertAsync(request, CenterId, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Rate saved"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Rate deactivated"));
    }
}
