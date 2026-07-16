namespace BloodCenterOS.Core.Models;

public class InventoryStock
{
    public long InventoryStockId { get; set; }
    public long? CenterId { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public int AvailableQty { get; set; }
    public int ReservedQty { get; set; }
    public int QuarantinedQty { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public long? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
