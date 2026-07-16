namespace BloodCenterOS.Core.Models;

public class IssueRecord
{
    public long IssueRecordId { get; set; }
    public long? CenterId { get; set; }
    public long? ComponentId { get; set; }
    public long? BagId { get; set; }
    public string? PatientName { get; set; }
    public long? HospitalId { get; set; }
    public DateTime IssueDate { get; set; }
    public long? IssuedByUserId { get; set; }
    public string? IssueType { get; set; }
    public string? IssueSlipNumber { get; set; }
    public long? RelatedBillingId { get; set; }
    public string? Notes { get; set; }
}
