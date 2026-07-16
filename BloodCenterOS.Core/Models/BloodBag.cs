namespace BloodCenterOS.Core.Models;

public class BloodBag
{
    public long BagId { get; set; }
    public long? CenterId { get; set; }
    public string BloodBagNumber { get; set; } = string.Empty;
    public long? CollectionId { get; set; }
    public long? DonorId { get; set; }
    public string? BagBarcode { get; set; }
    public string? BagLotNumber { get; set; }
    public decimal? BagVolumeMl { get; set; }
    public string? BagType { get; set; }
    public string? BagStatus { get; set; }
    public DateTime? InitialCollectedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? QuarantineReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
