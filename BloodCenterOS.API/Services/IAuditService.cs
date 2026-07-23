namespace BloodCenterOS.API.Services;

public interface IAuditService
{
    Task LogAsync(string tableName, string action, string? recordId, string? details,
        string? oldValue = null, string? newValue = null, long? propertyOwnerId = null);
}
