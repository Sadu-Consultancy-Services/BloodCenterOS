namespace BloodCenterOS.Core.Models;

public class Camp
{
    public long CampId { get; set; }
    public long? CenterId { get; set; }
    public string? CampCode { get; set; }
    public string? CampName { get; set; }
    public long? OrganizerId { get; set; }
    public string? Venue { get; set; }
    public string? City { get; set; }
    public DateTime? CampDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? TotalDonorsExpected { get; set; }
    public int? TotalDonorsCollected { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
