namespace BloodCenterOS.Core.Models;

public class EmergencyRequest
{
    public long EmergencyRequestId { get; set; }
    public long? CenterId { get; set; }
    public long? HospitalId { get; set; }
    public string? PatientName { get; set; }
    public string? BloodGroup { get; set; }
    public string? ComponentType { get; set; }
    public int? UnitsRequired { get; set; }
    public string? RequestStatus { get; set; }
    public DateTime RequestedAt { get; set; }
    public long? RequestedByUserId { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public string? Notes { get; set; }
}
