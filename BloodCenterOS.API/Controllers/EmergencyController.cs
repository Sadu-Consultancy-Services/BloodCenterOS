using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/emergency")]
public class EmergencyController : ControllerBase
{
    private readonly IEmergencyRepository _emergencyRepo;

    public EmergencyController(IEmergencyRepository emergencyRepo)
    {
        _emergencyRepo = emergencyRepo;
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPending()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<EmergencyRequest>>.Fail("Invalid center id"));

            var requests = await _emergencyRepo.GetPendingAsync(cid);
            return Ok(ApiResponse<IEnumerable<EmergencyRequest>>.Ok(requests));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<EmergencyRequest>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] EmergencyRequest request)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                request.CenterId = cid;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userId, out var uid))
                request.RequestedByUserId = uid;

            var id = await _emergencyRepo.CreateRequestAsync(request);
            request.EmergencyRequestId = id;
            return CreatedAtAction(null, ApiResponse<EmergencyRequest>.Ok(request, "Emergency request created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<EmergencyRequest>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
