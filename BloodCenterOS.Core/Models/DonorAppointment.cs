namespace BloodCenterOS.Core.Models;

public class DonorAppointment
{
    public long AppointmentId { get; set; }
    public long? CenterId { get; set; }
    public long DonorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Slot { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
