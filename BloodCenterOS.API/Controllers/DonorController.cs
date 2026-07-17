using System.Security.Claims;
using BloodCenterOS.Core.Models;
using Donation = BloodCenterOS.Core.Models.Donation;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/donors")]
public class DonorController : ControllerBase
{
    private readonly IDonorRepository _donorRepo;

    public DonorController(IDonorRepository donorRepo)
    {
        _donorRepo = donorRepo;
    }

    private long GetCenterId()
    {
        var claim = User.FindFirst("CenterId")?.Value;
        return long.TryParse(claim, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        try
        {
            var centerId = GetCenterId();
            var result = await _donorRepo.SearchAsync(centerId, null, null, null, page, size);
            return Ok(ApiResponse<PagedResult<Donor>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Donor>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var donor = await _donorRepo.GetByIdAsync(id);
            if (donor is null)
                return NotFound(ApiResponse<Donor>.Fail("Donor not found"));

            return Ok(ApiResponse<Donor>.Ok(donor));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Donor>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Donor donor)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                donor.CenterId = cid;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userId, out var uid))
                donor.CreatedBy = uid;

            var id = await _donorRepo.CreateAsync(donor);
            donor.DonorId = id;
            return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Donor>.Ok(donor, "Donor created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Donor>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Donor donor)
    {
        try
        {
            var existing = await _donorRepo.GetByIdAsync(id);
            if (existing is null)
                return NotFound(ApiResponse<Donor>.Fail("Donor not found"));

            donor.DonorId = id;
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                donor.CenterId = cid;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userId, out var uid))
                donor.UpdatedBy = uid;

            await _donorRepo.UpdateAsync(donor);
            return Ok(ApiResponse<Donor>.Ok(donor, "Donor updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Donor>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? bloodGroup,
        [FromQuery] string? gender,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            long? cid = long.TryParse(centerId, out var parsed) ? parsed : null;

            var result = await _donorRepo.SearchAsync(cid, keyword, bloodGroup, gender, page, size);
            return Ok(ApiResponse<PagedResult<Donor>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Donor>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{donorId}/donations")]
    public async Task<IActionResult> GetDonations(long donorId)
    {
        try
        {
            var donations = await _donorRepo.GetDonationHistoryByDonorAsync(donorId);
            return Ok(ApiResponse<IEnumerable<Donation>>.Ok(donations));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Donation>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("by-phone")]
    public async Task<IActionResult> GetByPhone([FromQuery] string phone)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Donor>>.Fail("Invalid center id"));

            var donors = await _donorRepo.GetByPhoneAsync(cid, phone);
            return Ok(ApiResponse<IEnumerable<Donor>>.Ok(donors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Donor>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
