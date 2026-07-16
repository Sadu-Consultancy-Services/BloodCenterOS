namespace BloodCenterOS.Core.Models;

public class AuditLog
{
    public long AuditLogId { get; set; }
    public long? PropertyOwnerId { get; set; }
    public long UserId { get; set; }
    public string? Action { get; set; }
    public string? TableName { get; set; }
    public string? RecordId { get; set; }
    public string? ActionDetails { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
