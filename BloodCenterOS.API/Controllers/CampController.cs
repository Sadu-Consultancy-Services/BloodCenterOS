using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/camps")]
public class CampController : ControllerBase
{
    private readonly ICampRepository _campRepo;

    public CampController(ICampRepository campRepo)
    {
        _campRepo = campRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Camp>>.Fail("Invalid center id"));

            var camps = await _campRepo.GetByCenterAsync(cid);
            return Ok(ApiResponse<IEnumerable<Camp>>.Ok(camps));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Camp>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var camp = await _campRepo.GetByIdAsync(id);
            if (camp is null)
                return NotFound(ApiResponse<Camp>.Fail("Camp not found"));

            return Ok(ApiResponse<Camp>.Ok(camp));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Camp>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Camp camp)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                camp.CenterId = cid;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<Camp>.Fail("Invalid user id"));

            var id = await _campRepo.CreateAsync(camp, uid);
            camp.CampId = id;
            return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Camp>.Ok(camp, "Camp created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Camp>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Camp>>.Fail("Invalid center id"));

            var camps = await _campRepo.GetUpcomingAsync(cid);
            return Ok(ApiResponse<IEnumerable<Camp>>.Ok(camps));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Camp>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
