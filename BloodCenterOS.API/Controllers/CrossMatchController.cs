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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCrossMatchRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.RequestId, request.ComponentId, request.Result, request.Method, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Cross-match recorded"));
    }
}

public class CreateCrossMatchRequest
{
    public long RequestId { get; set; }
    public long ComponentId { get; set; }
    public string? Result { get; set; }
    public string? Method { get; set; }
}
