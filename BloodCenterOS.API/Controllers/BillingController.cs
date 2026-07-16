using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingRepository _billingRepo;

    public BillingController(IBillingRepository billingRepo)
    {
        _billingRepo = billingRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Billing>>.Fail("Invalid center id"));

            var billings = await _billingRepo.GetByCenterAsync(cid);
            return Ok(ApiResponse<IEnumerable<Billing>>.Ok(billings));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Billing>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Billing billing)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                billing.CenterId = cid;

            var id = await _billingRepo.CreateBillingAsync(billing);
            billing.BillingTransactionId = id;
            return CreatedAtAction(null, ApiResponse<Billing>.Ok(billing, "Billing created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Billing>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost("{billingId}/payment")]
    public async Task<IActionResult> AddPayment(long billingId, [FromQuery] decimal amount, [FromQuery] string mode, [FromQuery] string? reference)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<long>.Fail("Invalid center id"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long? uid = long.TryParse(userId, out var parsed) ? parsed : null;

            var id = await _billingRepo.AddPaymentAsync(billingId, cid, amount, mode, reference, uid);
            return Ok(ApiResponse<long>.Ok(id, "Payment added successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<long>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
