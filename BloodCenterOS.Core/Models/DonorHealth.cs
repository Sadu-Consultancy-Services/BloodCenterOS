namespace BloodCenterOS.Core.Models;

public class DonorHealth
{
    public long DonorHealthHistoryId { get; set; }
    public long? CenterId { get; set; }
    public long DonorId { get; set; }
    public DateTime VisitDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? Temperature { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? Hemoglobin { get; set; }
    public int? PulseRate { get; set; }
    public string? Remarks { get; set; }
    public long? RecordedBy { get; set; }
}
