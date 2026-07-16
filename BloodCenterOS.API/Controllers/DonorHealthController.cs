using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/donors/{donorId}/health")]
public class DonorHealthController : ControllerBase
{
    private readonly IDonorHealthRepository _repo;
    public DonorHealthController(IDonorHealthRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Create(long donorId, [FromBody] CreateDonorHealthRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, donorId, request.WeightKg, request.Temperature, request.BloodPressure, request.Hemoglobin, request.PulseRate, request.Remarks, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Health record saved"));
    }

    [HttpGet]
    public async Task<IActionResult> GetByDonor(long donorId)
    {
        var data = await _repo.GetByDonorAsync(donorId);
        return Ok(ApiResponse<IEnumerable<DonorHealth>>.Ok(data));
    }
}

public class CreateDonorHealthRequest
{
    public decimal? WeightKg { get; set; }
    public decimal? Temperature { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? Hemoglobin { get; set; }
    public int? PulseRate { get; set; }
    public string? Remarks { get; set; }
}
