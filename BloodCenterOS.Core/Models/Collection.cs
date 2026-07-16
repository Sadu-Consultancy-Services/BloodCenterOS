namespace BloodCenterOS.Core.Models;

public class Collection
{
    public long CollectionId { get; set; }
    public long? CenterId { get; set; }
    public long? BranchId { get; set; }
    public long? CampId { get; set; }
    public long? DonorId { get; set; }
    public string? BloodBagNumber { get; set; }
    public string? BagBarcode { get; set; }
    public string? BagLotNumber { get; set; }
    public decimal? BagVolumeMl { get; set; }
    public long? CollectorEmployeeId { get; set; }
    public string? CollectionLocationType { get; set; }
    public DateTime? CollectionStartTime { get; set; }
    public DateTime? CollectionEndTime { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
