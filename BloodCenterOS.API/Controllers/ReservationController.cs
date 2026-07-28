using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservationRepository _repo;
    public ReservationController(IReservationRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate, [FromQuery] string? keyword)
    {
        var data = await _repo.GetAllAsync(CenterId, status, fromDate, toDate, keyword);
        return Ok(ApiResponse<IEnumerable<PatientReservation>>.Ok(data));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var data = await _repo.GetPendingAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<PatientReservation>>.Ok(data));
    }

    [HttpGet("available-components")]
    public async Task<IActionResult> GetAvailableComponents(
        [FromQuery] string bloodGroup, [FromQuery] string componentType, [FromQuery] int units = 1)
    {
        var data = await _repo.GetAvailableComponentsAsync(CenterId, bloodGroup, componentType, units);
        return Ok(ApiResponse<IEnumerable<AvailableComponentItem>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<PatientReservation>.Fail("Reservation not found"));
        var details = await _repo.GetDetailsAsync(id);
        item.Details = details.ToList();
        return Ok(ApiResponse<PatientReservation>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateRequest request)
    {
        if (string.IsNullOrEmpty(request.PatientName))
            return BadRequest(ApiResponse<object>.Fail("Patient name is required"));
        if (request.Units < 1)
            return BadRequest(ApiResponse<object>.Fail("At least 1 unit required"));

        var id = await _repo.CreateAsync(request, CenterId, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Reservation created"));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(long id, [FromBody] string? reason)
    {
        await _repo.CancelAsync(id, reason);
        return Ok(ApiResponse<object>.Ok(new { }, "Reservation cancelled"));
    }
}
