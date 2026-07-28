using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/issues")]
public class IssueController : ControllerBase
{
    private readonly IIssueRepository _repo;
    public IssueController(IIssueRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<IssueRecord>>.Ok(items));
    }

    [HttpPost("from-reservation")]
    public async Task<IActionResult> IssueFromReservation([FromBody] IssueFromReservationRequest req)
    {
        var count = await _repo.IssueFromReservationAsync(CenterId, req.ReservationId, req.PaymentMode, UserId, req.Notes);
        return Ok(ApiResponse<long>.Ok(count, "Issued successfully"));
    }

    [HttpGet("by-reservation/{reservationId}")]
    public async Task<IActionResult> GetByReservation(long reservationId)
    {
        var items = await _repo.GetByReservationAsync(reservationId);
        return Ok(ApiResponse<IEnumerable<IssueRecord>>.Ok(items));
    }

    [HttpGet("ready-for-issue")]
    public async Task<IActionResult> GetReadyForIssue()
    {
        var items = await _repo.GetReadyForIssueAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<ReservationReadyForIssue>>.Ok(items));
    }
}
