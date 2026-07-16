namespace BloodCenterOS.Core.Models;

public class Donor
{
    public long DonorId { get; set; }
    public long? CenterId { get; set; }
    public string? DonorCode { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BloodGroup { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AadhaarNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Pincode { get; set; }
    public string? Occupation { get; set; }
    public string? PreferredLanguage { get; set; }
    public DateTime? LastDonationDate { get; set; }
    public int TotalDonations { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
