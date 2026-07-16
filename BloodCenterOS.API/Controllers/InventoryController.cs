using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository _inventoryRepo;

    public InventoryController(IInventoryRepository inventoryRepo)
    {
        _inventoryRepo = inventoryRepo;
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<InventoryStock>>.Fail("Invalid center id"));

            var stock = await _inventoryRepo.GetStockAsync(cid);
            return Ok(ApiResponse<IEnumerable<InventoryStock>>.Ok(stock));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<InventoryStock>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<dynamic>>.Fail("Invalid center id"));

            var summary = await _inventoryRepo.GetSummaryAsync(cid);
            return Ok(ApiResponse<IEnumerable<dynamic>>.Ok(summary));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<dynamic>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromQuery] string? componentType,
        [FromQuery] string? bloodGroup,
        [FromQuery] int available,
        [FromQuery] int reserved,
        [FromQuery] int quarantined)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long? uid = long.TryParse(userId, out var parsed) ? parsed : null;

            var id = await _inventoryRepo.UpsertAsync(cid, componentType, bloodGroup, available, reserved, quarantined, uid);
            return Ok(ApiResponse<long>.Ok(id, "Stock adjusted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
