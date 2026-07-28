using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/discard")]
public class DiscardController : ControllerBase
{
    private readonly IDiscardRepository _repo;
    public DiscardController(IDiscardRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet("available-components")]
    public async Task<IActionResult> GetAvailableComponents()
    {
        var items = await _repo.GetAvailableComponentsAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<AvailableComponentForDiscard>>.Ok(items));
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkDiscard([FromBody] BulkDiscardRequest req)
    {
        var items = await _repo.BulkDiscardAsync(CenterId, req.ComponentIds, req.Reason, UserId, req.Notes);
        return Ok(ApiResponse<IEnumerable<DiscardRecord>>.Ok(items, $"{items.Count()} component(s) discarded"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? reason)
    {
        var items = await _repo.GetByCenterAsync(CenterId, from, to, reason);
        return Ok(ApiResponse<IEnumerable<DiscardRecord>>.Ok(items));
    }

    [HttpPut("autoclave")]
    public async Task<IActionResult> SetAutoclave([FromBody] SetAutoclaveRequest req)
    {
        await _repo.SetAutoclaveAsync(req.DiscardId, req.StartTime, req.EndTime);
        return Ok(ApiResponse<string>.Ok("Autoclave times recorded"));
    }

    [HttpGet("autoclave-register")]
    public async Task<IActionResult> GetAutoclaveRegister()
    {
        var items = await _repo.GetAutoclaveRegisterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<DiscardRecord>>.Ok(items));
    }
}
