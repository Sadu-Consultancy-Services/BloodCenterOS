using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _repo;
    public AppointmentController(IAppointmentRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? donorId)
    {
        var data = await _repo.GetAllAsync(CenterId, donorId);
        return Ok(ApiResponse<IEnumerable<DonorAppointment>>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.DonorId, request.Date, request.Slot, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Appointment created"));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest request)
    {
        await _repo.UpdateStatusAsync(id, request.Status);
        return Ok(ApiResponse<object>.Ok(new { }, "Status updated"));
    }
}

public class CreateAppointmentRequest
{
    public long DonorId { get; set; }
    public DateTime Date { get; set; }
    public string Slot { get; set; } = "";
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = "";
}
