namespace BloodCenterOS.Core.Models;

public class DiscardRecord
{
    public long DiscardId { get; set; }
    public long? CenterId { get; set; }
    public long? BagId { get; set; }
    public long? ComponentId { get; set; }
    public string? DiscardReason { get; set; }
    public DateTime DiscardedAt { get; set; }
    public long? DiscardedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime? AutoClaveStartTime { get; set; }
    public DateTime? AutoClaveEndTime { get; set; }

    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public string? BagNo { get; set; }
    public string? DonorName { get; set; }
}

public class AvailableComponentForDiscard
{
    public long ComponentId { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public int? VolumeMl { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long BagId { get; set; }
    public string? BagNo { get; set; }
    public long DonorId { get; set; }
    public string? DonorName { get; set; }
}

public class BulkDiscardRequest
{
    public long[] ComponentIds { get; set; } = Array.Empty<long>();
    public string Reason { get; set; } = "";
    public string? Notes { get; set; }
}

public class SetAutoclaveRequest
{
    public long DiscardId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
