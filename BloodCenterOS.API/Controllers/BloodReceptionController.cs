using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/blood-reception")]
public class BloodReceptionController : ControllerBase
{
    private readonly IBloodReceptionRepository _repo;
    public BloodReceptionController(IBloodReceptionRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var data = await _repo.GetAllByCenterAsync(CenterId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<BloodReception>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<BloodReception>.Fail("Reception not found"));
        var details = await _repo.GetDetailsAsync(id);
        item.Details = details.ToList();
        return Ok(ApiResponse<BloodReception>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BloodReceptionCreateRequest request)
    {
        if (request.Details == null || request.Details.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("At least one bag detail is required"));

        request.ReceivedBy ??= UserId;
        var id = await _repo.CreateAsync(request, CenterId);
        return Ok(ApiResponse<long>.Ok(id, "Blood reception created"));
    }
}
