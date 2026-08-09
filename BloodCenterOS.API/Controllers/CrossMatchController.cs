using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/crossmatches")]
public class CrossMatchController : ControllerBase
{
    private readonly ICrossMatchRepository _repo;
    public CrossMatchController(ICrossMatchRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartCrossMatchRequest req)
    {
        var id = await _repo.StartAsync(CenterId, req.BloodRequestId, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Cross-match started"));
    }

    [HttpPut("set-result")]
    public async Task<IActionResult> SetResult([FromBody] SetTestResultRequest req)
    {
        await _repo.SetTestResultAsync(req.TestResultId, req.Result);
        return Ok(ApiResponse<string>.Ok("Result updated"));
    }

    [HttpPost("reject-component/{testResultId}")]
    public async Task<IActionResult> RejectComponent(long testResultId)
    {
        await _repo.RejectComponentAsync(testResultId);
        return Ok(ApiResponse<string>.Ok("Component rejected"));
    }

    [HttpGet("pending-reservations")]
    public async Task<IActionResult> GetPendingReservations()
    {
        var items = await _repo.GetPendingReservationsAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<CrossMatchEntry>>.Ok(items));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var items = await _repo.GetByCenterAsync(CenterId, status, from, to);
        return Ok(ApiResponse<IEnumerable<CrossMatchEntry>>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entry = await _repo.GetByIdAsync(id);
        if (entry == null) return NotFound(ApiResponse<string>.Fail("Cross-match not found"));
        var tests = await _repo.GetTestsAsync(id);
        var result = new CrossMatchWithTests { Entry = entry, Tests = tests.ToList() };
        return Ok(ApiResponse<CrossMatchWithTests>.Ok(result));
    }
}
