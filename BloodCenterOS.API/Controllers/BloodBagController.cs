using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/blood-bags")]
public class BloodBagController : ControllerBase
{
    private readonly IBloodBagRepository _repo;
    public BloodBagController(IBloodBagRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet("{bagNo}")]
    public async Task<IActionResult> GetByNumber(string bagNo)
    {
        var bag = await _repo.GetByNumberAsync(bagNo);
        if (bag == null) return NotFound(ApiResponse<object>.Fail("Bag not found"));
        return Ok(ApiResponse<BloodBag>.Ok(bag));
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? term)
    {
        var data = await _repo.SearchAsync(CenterId, term);
        return Ok(ApiResponse<IEnumerable<BloodBag>>.Ok(data));
    }

    [HttpPut("{bagId}/status")]
    public async Task<IActionResult> UpdateStatus(long bagId, [FromBody] UpdateBagStatusRequest request)
    {
        await _repo.UpdateStatusAsync(bagId, request.Status);
        return Ok(ApiResponse<object>.Ok(new { }, "Status updated"));
    }
}

public class UpdateBagStatusRequest
{
    public string Status { get; set; } = "";
}
