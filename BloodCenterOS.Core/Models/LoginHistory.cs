namespace BloodCenterOS.Core.Models;

public class LoginHistory
{
    public long LoginHistoryId { get; set; }
    public long UserId { get; set; }
    public long? CenterId { get; set; }
    public DateTime LoginAt { get; set; }
    public DateTime? LogoutAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
