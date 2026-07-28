using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mbb-bills")]
public class MbbBillController : ControllerBase
{
    private readonly IMbbBillRepository _repo;
    public MbbBillController(IMbbBillRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<MbbBill>>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var bill = await _repo.GetByIdAsync(id);
        if (bill == null) return NotFound(ApiResponse<string>.Fail("MBB bill not found"));
        var details = await _repo.GetDetailAsync(id);
        var result = new MbbBillWithDetails { Bill = bill, Details = details.ToList() };
        return Ok(ApiResponse<MbbBillWithDetails>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMbbBillRequest req)
    {
        var id = await _repo.CreateBillAsync(CenterId, req, UserId);
        return Ok(ApiResponse<long>.Ok(id, "MBB bill created"));
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> MakePayment(long id, [FromQuery] decimal amount, [FromQuery] string mode)
    {
        await _repo.MakePaymentAsync(id, amount, mode, UserId);
        return Ok(ApiResponse<string>.Ok("Payment recorded"));
    }
}
