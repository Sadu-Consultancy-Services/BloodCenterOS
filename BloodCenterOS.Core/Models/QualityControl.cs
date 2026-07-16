namespace BloodCenterOS.Core.Models;

public class QualityControl
{
    public long QcRecordId { get; set; }
    public long? CenterId { get; set; }
    public long DeviceId { get; set; }
    public DateTime QcDate { get; set; }
    public string? QcDetail { get; set; }
    public long? PerformedBy { get; set; }
}
