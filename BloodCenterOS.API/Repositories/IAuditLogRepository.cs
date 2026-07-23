using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAsync(long? userId, string? tableName, int limit = 100);
    Task CreateAsync(AuditLog entry);
}
