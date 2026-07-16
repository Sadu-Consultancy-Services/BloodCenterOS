using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/quality-control")]
public class QualityControlController : ControllerBase
{
    private readonly IQualityControlRepository _repo;
    public QualityControlController(IQualityControlRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQcRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.DeviceId, request.Detail, UserId);
        return Ok(ApiResponse<long>.Ok(id, "QC record saved"));
    }
}

public class CreateQcRequest
{
    public long DeviceId { get; set; }
    public string Detail { get; set; } = "";
}
