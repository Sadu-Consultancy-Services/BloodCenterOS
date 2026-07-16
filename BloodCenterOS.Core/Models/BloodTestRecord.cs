namespace BloodCenterOS.Core.Models;

public class BloodTestRecord
{
    public long TestRecordId { get; set; }
    public long? CenterId { get; set; }
    public long? CollectionId { get; set; }
    public string? BagNumber { get; set; }
    public DateTime? SampleTakenAt { get; set; }
    public long? PerformedBy { get; set; }
    public string? OverallStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StatusBadge => OverallStatus switch
    {
        "Pending" => "badge-warning",
        "In Progress" => "badge-info",
        "Completed" or "Negative" => "badge-success",
        "Positive" or "Reactive" => "badge-danger",
        _ => "badge-secondary"
    };
}
