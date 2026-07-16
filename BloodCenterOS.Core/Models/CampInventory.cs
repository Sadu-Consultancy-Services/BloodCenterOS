namespace BloodCenterOS.Core.Models;

public class CampInventory
{
    public long CampInventoryId { get; set; }
    public long CampId { get; set; }
    public string? CampName { get; set; }
    public string? ItemName { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime? CreatedAt { get; set; }
}
