using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/camp-expenses")]
public class CampExpenseController : ControllerBase
{
    private readonly ICampExpenseRepository _repo;
    public CampExpenseController(ICampExpenseRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? campId)
    {
        if (campId.HasValue)
        {
            var data = await _repo.GetByCampAsync(campId.Value);
            return Ok(ApiResponse<IEnumerable<CampExpense>>.Ok(data));
        }
        var all = await _repo.GetByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<CampExpense>>.Ok(all));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampExpenseRequest request)
    {
        var id = await _repo.CreateAsync(request.CampId, request.ExpenseCategory, request.Amount, request.Notes);
        return Ok(ApiResponse<long>.Ok(id, "Expense recorded"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCampExpenseRequest request)
    {
        await _repo.UpdateAsync(id, request.ExpenseCategory, request.Amount, request.Notes);
        return Ok(ApiResponse<object>.Ok(new { }, "Expense updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Expense deleted"));
    }
}

public class CreateCampExpenseRequest
{
    public long CampId { get; set; }
    public string ExpenseCategory { get; set; } = "";
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCampExpenseRequest
{
    public string? ExpenseCategory { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}
