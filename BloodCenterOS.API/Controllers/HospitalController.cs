using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/hospitals")]
public class HospitalController : ControllerBase
{
    private readonly IHospitalRepository _hospitalRepo;

    public HospitalController(IHospitalRepository hospitalRepo)
    {
        _hospitalRepo = hospitalRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Hospital>>.Fail("Invalid center id"));

            var hospitals = await _hospitalRepo.GetAllByCenterAsync(cid);
            return Ok(ApiResponse<IEnumerable<Hospital>>.Ok(hospitals));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Hospital>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Hospital hospital)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                hospital.CenterId = cid;

            var id = await _hospitalRepo.CreateAsync(hospital);
            hospital.HospitalId = id;
            return CreatedAtAction(null, ApiResponse<Hospital>.Ok(hospital, "Hospital created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Hospital>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
