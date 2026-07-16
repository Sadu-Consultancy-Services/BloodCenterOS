namespace BloodCenterOS.Core.Models;

public class DonorCommunication
{
    public long CommId { get; set; }
    public long? CenterId { get; set; }
    public long DonorId { get; set; }
    public string? Channel { get; set; }
    public string? Message { get; set; }
    public DateTime SentAt { get; set; }
    public long? SentBy { get; set; }
    public string? Status { get; set; }
}
