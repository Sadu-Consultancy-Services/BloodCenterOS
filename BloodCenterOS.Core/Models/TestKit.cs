namespace BloodCenterOS.Core.Models;

public class TestKit
{
    public long TestKitId { get; set; }
    public long? CenterId { get; set; }
    public string KitName { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
