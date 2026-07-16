namespace BloodCenterOS.Core.Models;

public class ReturnRecord
{
    public long ReturnId { get; set; }
    public long? CenterId { get; set; }
    public long IssueRecordId { get; set; }
    public long ComponentId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public long? CreatedBy { get; set; }
}
