using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/expenses")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseRepository _repo;
    public ExpenseController(IExpenseRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var data = await _repo.GetAllAsync(CenterId, from, to);
        return Ok(ApiResponse<IEnumerable<Expense>>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.Category, request.Amount, request.Notes, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Expense recorded"));
    }
}

public class CreateExpenseRequest
{
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
