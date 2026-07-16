namespace BloodCenterOS.Core.Models;

public class BloodTestResult
{
    public long TestResultId { get; set; }
    public long? CenterId { get; set; }
    public long? TestRecordId { get; set; }
    public long? BagId { get; set; }
    public string TestCode { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Method { get; set; }
    public string? KitLotNo { get; set; }
    public long? PerformedBy { get; set; }
    public DateTime? PerformedAt { get; set; }
    public string? Remarks { get; set; }
}
