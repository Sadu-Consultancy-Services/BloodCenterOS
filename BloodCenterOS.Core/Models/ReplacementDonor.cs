namespace BloodCenterOS.Core.Models;

public class ReplacementDonor
{
    public long ReplacementDonorId { get; set; }
    public long? CenterId { get; set; }
    public long PatientRequestId { get; set; }
    public long DonorId { get; set; }
    public DateTime? DonatedAt { get; set; }
}
