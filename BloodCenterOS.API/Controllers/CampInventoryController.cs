using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/camp-inventory")]
public class CampInventoryController : ControllerBase
{
    private readonly ICampInventoryRepository _repo;
    public CampInventoryController(ICampInventoryRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? campId)
    {
        if (campId.HasValue)
        {
            var data = await _repo.GetByCampAsync(campId.Value);
            return Ok(ApiResponse<IEnumerable<CampInventory>>.Ok(data));
        }
        var all = await _repo.GetByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<CampInventory>>.Ok(all));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampInventoryRequest request)
    {
        var id = await _repo.CreateAsync(request.CampId, request.ItemName, request.Quantity, request.Unit);
        return Ok(ApiResponse<long>.Ok(id, "Inventory item added"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCampInventoryRequest request)
    {
        await _repo.UpdateAsync(id, request.ItemName, request.Quantity, request.Unit);
        return Ok(ApiResponse<object>.Ok(new { }, "Item updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Item deleted"));
    }
}

public class CreateCampInventoryRequest
{
    public long CampId { get; set; }
    public string ItemName { get; set; } = "";
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
}

public class UpdateCampInventoryRequest
{
    public string? ItemName { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
}
