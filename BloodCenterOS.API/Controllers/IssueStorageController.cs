using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/issue-storage")]
public class IssueStorageController : ControllerBase
{
    private readonly IIssueStorageRepository _repo;
    public IssueStorageController(IIssueStorageRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet("available-components")]
    public async Task<IActionResult> GetAvailableComponents()
    {
        var items = await _repo.GetAvailableComponentsAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<AvailableComponentForStorage>>.Ok(items));
    }

    [HttpGet("rate/{storageId}/{componentType}")]
    public async Task<IActionResult> GetRate(long storageId, string componentType)
    {
        var rate = await _repo.GetStorageRateAsync(storageId, componentType);
        return Ok(ApiResponse<decimal>.Ok(rate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IssueToStorageRequest req)
    {
        var invoiceId = await _repo.CreateIssueAsync(CenterId, req, UserId);
        return Ok(ApiResponse<long>.Ok(invoiceId, "Issued to storage successfully"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? storageId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var items = await _repo.GetByCenterAsync(CenterId, storageId, from, to);
        return Ok(ApiResponse<IEnumerable<IssueStorageRecord>>.Ok(items));
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] long? storageId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var items = await _repo.GetInvoicesAsync(CenterId, storageId, from, to);
        return Ok(ApiResponse<IEnumerable<IssueStorageInvoice>>.Ok(items));
    }
}
