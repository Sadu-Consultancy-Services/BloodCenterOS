namespace BloodCenterOS.Core.Models;

public class Donation
{
    public long DonationId { get; set; }
    public long? CenterId { get; set; }
    public long DonorId { get; set; }
    public long? CollectionId { get; set; }
    public DateTime DonationDate { get; set; }
    public string? DonationType { get; set; }
    public decimal? VolumeMl { get; set; }
    public string? BagNumber { get; set; }
    public string? Remarks { get; set; }
    public long? CreatedBy { get; set; }
}
