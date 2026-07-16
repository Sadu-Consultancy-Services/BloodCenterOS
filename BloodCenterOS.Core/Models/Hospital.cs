namespace BloodCenterOS.Core.Models;

public class Hospital
{
    public long HospitalId { get; set; }
    public long? CenterId { get; set; }
    public string? HospitalCode { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
