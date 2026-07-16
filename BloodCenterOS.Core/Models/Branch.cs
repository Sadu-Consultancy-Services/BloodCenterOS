namespace BloodCenterOS.Core.Models;

public class Branch
{
    public long BranchId { get; set; }
    public long? CenterId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
