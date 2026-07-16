using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/components")]
public class ComponentLogController : ControllerBase
{
    private readonly IComponentLogRepository _repo;
    public ComponentLogController(IComponentLogRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost("{componentId}/store")]
    public async Task<IActionResult> Store(long componentId, [FromBody] StoreComponentRequest request)
    {
        var id = await _repo.StoreAsync(CenterId, componentId, request.FridgeId, request.Location, request.Notes);
        return Ok(ApiResponse<long>.Ok(id, "Component stored"));
    }

    [HttpPost("{componentId}/transfer")]
    public async Task<IActionResult> Transfer(long componentId, [FromBody] TransferComponentRequest request)
    {
        var id = await _repo.TransferAsync(CenterId, componentId, request.ToCenterId, request.TransportDetails, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Component transferred"));
    }

    [HttpPost("{componentId}/discard")]
    public async Task<IActionResult> Discard(long componentId, [FromBody] DiscardComponentRequest request)
    {
        var id = await _repo.DiscardAsync(CenterId, request.BagId, componentId, request.Reason, UserId, request.Notes);
        return Ok(ApiResponse<long>.Ok(id, "Component discarded"));
    }

    [HttpPut("{componentId}/status")]
    public async Task<IActionResult> UpdateStatus(long componentId, [FromBody] UpdateComponentStatusRequest request)
    {
        await _repo.UpdateStatusAsync(componentId, request.Status);
        return Ok(ApiResponse<object>.Ok(new { }, "Status updated"));
    }
}

public class StoreComponentRequest
{
    public long FridgeId { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class TransferComponentRequest
{
    public long ToCenterId { get; set; }
    public string? TransportDetails { get; set; }
}

public class DiscardComponentRequest
{
    public long BagId { get; set; }
    public string Reason { get; set; } = "";
    public string? Notes { get; set; }
}

public class UpdateComponentStatusRequest
{
    public string Status { get; set; } = "";
}
