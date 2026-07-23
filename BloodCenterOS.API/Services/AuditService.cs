using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repo;
    private readonly IHttpContextAccessor _http;

    public AuditService(IAuditLogRepository repo, IHttpContextAccessor http)
    {
        _repo = repo;
        _http = http;
    }

    public async Task LogAsync(string tableName, string action, string? recordId, string? details,
        string? oldValue = null, string? newValue = null, long? propertyOwnerId = null)
    {
        var ctx = _http.HttpContext;
        if (ctx == null) return;

        var userId = long.TryParse(ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : 0;

        var entry = new AuditLog
        {
            PropertyOwnerId = propertyOwnerId ?? 0,
            UserId = userId,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            ActionDetails = details,
            OldValue = oldValue,
            NewValue = newValue,
            IpAddress = ctx.Connection.RemoteIpAddress?.ToString(),
            UserAgent = ctx.Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.CreateAsync(entry);
    }
}
