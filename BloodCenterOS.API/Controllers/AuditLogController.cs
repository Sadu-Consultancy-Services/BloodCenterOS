using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _repo;
    public AuditLogController(IAuditLogRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] long? userId, [FromQuery] string? tableName, [FromQuery] int limit = 100)
    {
        var data = await _repo.GetAsync(userId, tableName, limit);
        return Ok(ApiResponse<IEnumerable<AuditLog>>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AuditLog entry)
    {
        await _repo.CreateAsync(entry);
        return Ok(ApiResponse<object>.Ok(new { }, "Audit log created"));
    }
}
