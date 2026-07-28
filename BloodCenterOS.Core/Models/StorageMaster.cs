namespace BloodCenterOS.Core.Models;

public class StorageMaster
{
    public long StorageId { get; set; }
    public long CenterId { get; set; }
    public string StorageName { get; set; } = "";
    public string? Address { get; set; }
    public string? PhoneNo { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public decimal RateWB { get; set; }
    public decimal RatePCV { get; set; }
    public decimal RateFFP { get; set; }
    public decimal RatePltsConc { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class IssueStorageRecord
{
    public long IssueStorageId { get; set; }
    public long StorageId { get; set; }
    public string? StorageName { get; set; }
    public long ComponentId { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public string? BagNo { get; set; }
    public long? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime IssueDateTime { get; set; }
    public decimal Rate { get; set; }
}

public class IssueStorageInvoice
{
    public long BillingTransactionId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime IssueDateTime { get; set; }
    public string? StorageName { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMode { get; set; }
    public decimal? Discount { get; set; }
    public long ComponentCount { get; set; }
}

public class IssueToStorageRequest
{
    public long StorageId { get; set; }
    public long[] ComponentIds { get; set; } = Array.Empty<long>();
    public DateTime IssueDate { get; set; } = DateTime.Now;
    public string? PaymentMode { get; set; } = "Credit";
    public decimal Discount { get; set; }
    public string? DiscountReason { get; set; }
    public decimal EmAmt { get; set; }
    public string? Notes { get; set; }
}

public class AvailableComponentForStorage
{
    public long ComponentId { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public int? VolumeMl { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long BagId { get; set; }
    public string? BagNo { get; set; }
}
