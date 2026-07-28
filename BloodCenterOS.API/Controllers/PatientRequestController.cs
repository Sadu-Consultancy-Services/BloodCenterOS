using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/patient-requests")]
public class PatientRequestController : ControllerBase
{
    private readonly IPatientRequestRepository _repo;
    public PatientRequestController(IPatientRequestRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<PatientRequest>>.Ok(data));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var data = await _repo.GetPendingAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<PatientRequest>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var data = await _repo.GetByIdAsync(CenterId, id);
        if (data == null) return NotFound(ApiResponse<object>.Fail("Patient request not found"));
        return Ok(ApiResponse<PatientRequest>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.HospitalId, request.PatientName, request.Age,
            request.Gender, request.BloodGroup, request.ComponentType, request.Units, request.Urgency, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Patient request created"));
    }
}

public class CreatePatientRequest
{
    public long? HospitalId { get; set; }
    public string PatientName { get; set; } = "";
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string BloodGroup { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public int Units { get; set; }
    public string Urgency { get; set; } = "Normal";
}