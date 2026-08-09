namespace BloodCenterOS.Core.Models;

public class IssueRecord
{
    public long IssueRecordId { get; set; }
    public long CenterId { get; set; }
    public long ComponentId { get; set; }
    public long BagId { get; set; }
    public string PatientName { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public long? IssuedByUserId { get; set; }
    public string IssueType { get; set; } = "Patient";
    public string? Notes { get; set; }
    public long? RelatedBillingId { get; set; }

    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
}

public class IssueFromReservationRequest
{
    public long BloodRequestId { get; set; }
    public string? PaymentMode { get; set; }
    public string? Notes { get; set; }
}

public class ReservationReadyForIssue
{
    public long BloodRequestId { get; set; }
    public string PatientName { get; set; } = "";
    public string RequiredBloodGroup { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public int UnitsReserved { get; set; }
    public string? HospitalName { get; set; }
    public long CrossMatchEntryId { get; set; }
    public string OverallResult { get; set; } = "";
}
