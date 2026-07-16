namespace BloodCenterOS.Core.Models;

public class Component
{
    public long ComponentId { get; set; }
    public long? CenterId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public long? ParentBagId { get; set; }
    public string? ComponentType { get; set; }
    public decimal? VolumeMl { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? StorageLocation { get; set; }
    public string? CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
