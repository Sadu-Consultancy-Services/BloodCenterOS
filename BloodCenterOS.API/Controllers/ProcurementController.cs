using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/procurement")]
public class ProcurementController : ControllerBase
{
    private readonly IProcurementRepository _repo;
    public ProcurementController(IProcurementRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet("register")]
    public async Task<IActionResult> Search(
        [FromQuery] string? bloodGroup,
        [FromQuery] string? componentType,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? keyword)
    {
        var data = await _repo.SearchAsync(CenterId, bloodGroup, componentType, status, fromDate, toDate, keyword);
        return Ok(ApiResponse<IEnumerable<ProcurementRegisterItem>>.Ok(data));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var data = await _repo.GetSummaryAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<ProcurementRegisterSummaryRow>>.Ok(data));
    }
}
