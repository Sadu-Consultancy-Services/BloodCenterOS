using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/login-history")]
public class LoginHistoryController : ControllerBase
{
    private readonly ILoginHistoryRepository _repo;
    public LoginHistoryController(ILoginHistoryRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] long? userId, [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate, [FromQuery] int limit = 200)
    {
        var data = await _repo.GetFilteredAsync(userId, fromDate, toDate, limit);
        return Ok(ApiResponse<IEnumerable<LoginHistory>>.Ok(data));
    }

    [HttpPost("{loginId}/logout")]
    public async Task<IActionResult> Logout(long loginId)
    {
        await _repo.LogoutAsync(loginId);
        return Ok(ApiResponse<object>.Ok(new { }, "Logged out"));
    }
}
