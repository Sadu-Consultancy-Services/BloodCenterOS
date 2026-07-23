namespace BloodCenterOS.Core.Models;

public class CampOrganizer
{
    public long OrganizerId { get; set; }
    public long CenterId { get; set; }
    public string? OrganizerName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}
