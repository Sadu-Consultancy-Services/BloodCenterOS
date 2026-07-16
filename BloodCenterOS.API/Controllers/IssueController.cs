using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/issues")]
public class IssueController : ControllerBase
{
    private readonly IIssueRepository _issueRepo;

    public IssueController(IIssueRepository issueRepo)
    {
        _issueRepo = issueRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<IssueRecord>>.Fail("Invalid center id"));

            var issues = await _issueRepo.GetByCenterAsync(cid);
            return Ok(ApiResponse<IEnumerable<IssueRecord>>.Ok(issues));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<IssueRecord>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IssueRecord issue)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                issue.CenterId = cid;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userId, out var uid))
                issue.IssuedByUserId = uid;

            var id = await _issueRepo.CreateIssueAsync(issue);
            issue.IssueRecordId = id;
            return CreatedAtAction(null, ApiResponse<IssueRecord>.Ok(issue, "Issue created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IssueRecord>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("pending-requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<PatientRequest>>.Fail("Invalid center id"));

            var requests = await _issueRepo.GetPendingRequestsAsync(cid);
            return Ok(ApiResponse<IEnumerable<PatientRequest>>.Ok(requests));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<PatientRequest>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
