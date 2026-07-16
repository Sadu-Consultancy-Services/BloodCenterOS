using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/test-kits")]
public class TestKitController : ControllerBase
{
    private readonly ITestKitRepository _repo;
    public TestKitController(ITestKitRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAvailable()
    {
        var data = await _repo.GetAvailableAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<TestKit>>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestKitRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.KitName, request.Manufacturer, request.LotNumber, request.ExpiryDate);
        return Ok(ApiResponse<long>.Ok(id, "Test kit added"));
    }
}

public class CreateTestKitRequest
{
    public string KitName { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
