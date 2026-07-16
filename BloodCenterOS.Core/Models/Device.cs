namespace BloodCenterOS.Core.Models;

public class Device
{
    public long DeviceId { get; set; }
    public long? CenterId { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public string? SerialNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyEndDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
