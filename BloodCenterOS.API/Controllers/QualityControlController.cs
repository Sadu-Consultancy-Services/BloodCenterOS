using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/quality-control")]
public class QualityControlController : ControllerBase
{
    private readonly IQualityControlRepository _repo;
    public QualityControlController(IQualityControlRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var items = await _repo.GetByCenterAsync(CenterId, type, from, to);
        return Ok(ApiResponse<IEnumerable<QualityControl>>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<string>.Fail("QC record not found"));
        return Ok(ApiResponse<QualityControl>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQcRequest req)
    {
        var id = await _repo.CreateAsync(CenterId, req, UserId);
        return Ok(ApiResponse<long>.Ok(id, "QC record saved"));
    }
}
