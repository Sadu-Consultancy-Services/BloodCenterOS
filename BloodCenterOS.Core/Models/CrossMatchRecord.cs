namespace BloodCenterOS.Core.Models;

public class CrossMatchRecord
{
    public long CrossMatchId { get; set; }
    public long? CenterId { get; set; }
    public long RequestId { get; set; }
    public long ComponentId { get; set; }
    public string? Result { get; set; }
    public string? Method { get; set; }
    public long? PerformedBy { get; set; }
    public DateTime? PerformedAt { get; set; }
}
