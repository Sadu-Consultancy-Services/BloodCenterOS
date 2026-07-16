namespace BloodCenterOS.Core.Models;

public class DeferralRecord
{
    public long DeferralId { get; set; }
    public long? CenterId { get; set; }
    public long DonorId { get; set; }
    public DateTime DeferralDate { get; set; }
    public string? Reason { get; set; }
    public DateTime? DeferralUntil { get; set; }
    public string? Notes { get; set; }
    public long? CreatedBy { get; set; }
}
