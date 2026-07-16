namespace BloodCenterOS.Core.Models;

public class PatientRequest
{
    public long RequestId { get; set; }
    public long? CenterId { get; set; }
    public long? HospitalId { get; set; }
    public string? PatientName { get; set; }
    public int? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string? BloodGroup { get; set; }
    public string? ComponentType { get; set; }
    public int? UnitsRequested { get; set; }
    public DateTime RequestDate { get; set; }
    public string? RequestUrgency { get; set; }
    public long? PrescriptionAttachmentId { get; set; }
    public long? RequestedByUserId { get; set; }
    public long? RelatedIssueId { get; set; }
}
