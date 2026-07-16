using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tests")]
public class TestController : ControllerBase
{
    private readonly ITestRepository _testRepo;

    public TestController(ITestRepository testRepo)
    {
        _testRepo = testRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BloodTestRecord record)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<long>.Fail("Invalid user id"));

            var id = await _testRepo.CreateRecordAsync(cid, record.CollectionId, record.BagNumber, uid);
            return Ok(ApiResponse<long>.Ok(id, "Test record created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<BloodTestRecord>>.Fail("Invalid center id"));

            var records = await _testRepo.GetPendingAsync(cid);
            return Ok(ApiResponse<IEnumerable<BloodTestRecord>>.Ok(records));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<BloodTestRecord>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var record = await _testRepo.GetRecordByIdAsync(id);
            if (record is null)
                return NotFound(ApiResponse<BloodTestRecord>.Fail("Test record not found"));

            return Ok(ApiResponse<BloodTestRecord>.Ok(record));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BloodTestRecord>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}/results")]
    public async Task<IActionResult> GetResults(long id)
    {
        try
        {
            var results = await _testRepo.GetResultsByRecordAsync(id);
            return Ok(ApiResponse<IEnumerable<BloodTestResult>>.Ok(results));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<BloodTestResult>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("{id}/results")]
    public async Task<IActionResult> AddResult(long id, [FromBody] BloodTestResult result)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<long>.Fail("Invalid user id"));

            result.TestRecordId = id;
            result.CenterId = cid;
            result.PerformedBy = uid;

            var resultId = await _testRepo.AddResultAsync(result);
            return Ok(ApiResponse<long>.Ok(resultId, "Test result added successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(long id)
    {
        try
        {
            await _testRepo.UpdateRecordStatusAsync(id, "Completed");
            return Ok(ApiResponse<object>.Ok(new { }, "Test record completed"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
