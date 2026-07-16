namespace BloodCenterOS.Core.Models;

public class Notification
{
    public long NotificationId { get; set; }
    public long? CenterId { get; set; }
    public string? NotificationType { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? TargetAudience { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
