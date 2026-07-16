using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/components")]
public class ComponentController : ControllerBase
{
    private readonly IComponentRepository _componentRepo;

    public ComponentController(IComponentRepository componentRepo)
    {
        _componentRepo = componentRepo;
    }

    [HttpPost("prepare")]
    public async Task<IActionResult> Prepare([FromQuery] long bagId, [FromQuery] string componentType, [FromQuery] int volume)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<long>.Fail("Invalid user id"));

            var id = await _componentRepo.PrepareAsync(cid, bagId, componentType, volume, uid);
            return Ok(ApiResponse<long>.Ok(id, "Component prepared successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] string? bloodGroup)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Component>>.Fail("Invalid center id"));

            var components = await _componentRepo.GetAvailableAsync(cid, bloodGroup);
            return Ok(ApiResponse<IEnumerable<Component>>.Ok(components));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Component>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> Transfer(long id, [FromQuery] long toCenterId, [FromQuery] string? transportDetails)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<long>.Fail("Invalid user id"));

            var transferId = await _componentRepo.TransferAsync(cid, id, toCenterId, transportDetails, uid);
            return Ok(ApiResponse<long>.Ok(transferId, "Component transferred successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("discard")]
    public async Task<IActionResult> Discard([FromQuery] long? bagId, [FromQuery] long? componentId, [FromQuery] string reason, [FromQuery] string? notes)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<long>.Fail("Invalid user id"));

            var id = await _componentRepo.DiscardAsync(cid, bagId, componentId, reason, uid, notes);
            return Ok(ApiResponse<long>.Ok(id, "Component discarded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
