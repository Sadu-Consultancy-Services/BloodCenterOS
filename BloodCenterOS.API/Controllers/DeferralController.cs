using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/deferrals")]
public class DeferralController : ControllerBase
{
    private readonly IDeferralRepository _repo;
    public DeferralController(IDeferralRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeferralRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.DonorId, request.Reason, request.Until, request.Notes, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Donor deferred"));
    }

    [HttpGet("active/{donorId}")]
    public async Task<IActionResult> GetActive(long donorId)
    {
        var data = await _repo.GetActiveAsync(donorId);
        return Ok(ApiResponse<IEnumerable<DeferralRecord>>.Ok(data));
    }
}

public class CreateDeferralRequest
{
    public long DonorId { get; set; }
    public string Reason { get; set; } = "";
    public DateTime? Until { get; set; }
    public string? Notes { get; set; }
}
