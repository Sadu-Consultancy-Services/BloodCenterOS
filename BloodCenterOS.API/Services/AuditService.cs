using System.Net;
using System.Net.Sockets;
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
            IpAddress = NormalizeIp(ctx.Connection.RemoteIpAddress),
            UserAgent = ctx.Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.CreateAsync(entry);
    }

    private static string? NormalizeIp(IPAddress? address)
    {
        if (address == null) return null;
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();
            else if (address.Equals(IPAddress.IPv6Loopback))
                address = IPAddress.Loopback;
        }
        return address.ToString();
    }
}
