namespace BloodCenterOS.Core.Models;

public class ProcurementRegisterItem
{
    public long RegisterId { get; set; }
    public long ComponentId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string? ComponentType { get; set; }
    public int? VolumeMl { get; set; }
    public string? BloodGroup { get; set; }
    public string? BagNumber { get; set; }
    public string? BagType { get; set; }
    public string? DonorName { get; set; }
    public long? DonorId { get; set; }
    public string? Status { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? StorageLocation { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProcurementRegisterSummaryRow
{
    public string BloodGroup { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Reserved { get; set; }
    public int Issued { get; set; }
    public int Discarded { get; set; }
    public int Total { get; set; }
}
