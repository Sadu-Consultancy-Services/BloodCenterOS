namespace BloodCenterOS.Core.Models;

public class BloodReception
{
    public long ReceptionId { get; set; }
    public long CenterId { get; set; }
    public string MBBName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string? BillNumber { get; set; }
    public int TotalBags { get; set; }
    public string? Notes { get; set; }
    public long? ReceivedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BloodReceptionDetail> Details { get; set; } = new();
}

public class BloodReceptionDetail
{
    public long ReceptionDetailId { get; set; }
    public long ReceptionId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? Sex { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string BagNumber { get; set; } = string.Empty;
    public string BagType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int VolumeMl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BloodReceptionCreateRequest
{
    public string MBBName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string? BillNumber { get; set; }
    public string? Notes { get; set; }
    public long? ReceivedBy { get; set; }
    public List<BloodReceptionDetailRequest> Details { get; set; } = new();
}

public class BloodReceptionDetailRequest
{
    public string DonorName { get; set; } = string.Empty;
    public string? Sex { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string BagNumber { get; set; } = string.Empty;
    public string BagType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int VolumeMl { get; set; } = 350;
}
