namespace BloodCenterOS.Core.Models;

public class EmergencyDonorResponse
{
    public long ResponseId { get; set; }
    public long EmergencyRequestId { get; set; }
    public long DonorId { get; set; }
    public string? ResponseContact { get; set; }
    public DateTime RespondedAt { get; set; }
    public bool IsVerified { get; set; }
}
