using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/returns")]
public class ReturnController : ControllerBase
{
    private readonly IReturnRepository _repo;
    public ReturnController(IReturnRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.IssueRecordId, request.ComponentId, request.Reason, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Return recorded"));
    }
}

public class CreateReturnRequest
{
    public long IssueRecordId { get; set; }
    public long ComponentId { get; set; }
    public string Reason { get; set; } = "";
}
