namespace BloodCenterOS.Core.Models;

public class DonorSummaryRow
{
    public string Period { get; set; } = "";
    public long TotalRegistered { get; set; }
    public long TotalBloodGroupAPositive { get; set; }
    public long TotalBloodGroupANegative { get; set; }
    public long TotalBloodGroupBPositive { get; set; }
    public long TotalBloodGroupBNegative { get; set; }
    public long TotalBloodGroupAbPositive { get; set; }
    public long TotalBloodGroupAbNegative { get; set; }
    public long TotalBloodGroupOPositive { get; set; }
    public long TotalBloodGroupONegative { get; set; }
    public long TotalDeferrals { get; set; }
    public long TotalCollections { get; set; }
}

public class InventorySummaryRow
{
    public string ComponentType { get; set; } = "";
    public string BloodGroup { get; set; } = "";
    public long AvailableQty { get; set; }
    public long ReservedQty { get; set; }
    public long QuarantinedQty { get; set; }
    public long NearExpiryQty { get; set; }
}

public class CampSummaryRow
{
    public string Period { get; set; } = "";
    public long TotalCamps { get; set; }
    public long TotalExpected { get; set; }
    public long TotalCollected { get; set; }
    public decimal CollectionRate { get; set; }
}

public class CenterConfigItem
{
    public string ConfigKey { get; set; } = "";
    public string? ConfigValue { get; set; }
}

public class SystemConfigItem
{
    public string ConfigKey { get; set; } = "";
    public string? ConfigValue { get; set; }
    public string? Description { get; set; }
}

public class LookupTypeItem
{
    public long LookupTypeId { get; set; }
    public string TypeCode { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
}

public class LookupValueItem
{
    public long LookupValueId { get; set; }
    public long? LookupTypeId { get; set; }
    public string ValueCode { get; set; } = "";
    public string ValueText { get; set; } = "";
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

// ── Phase 9 Report Models ──

public class BloodStockRow
{
    public string BloodGroup { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public long AvailableQty { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class ProcurementSummaryRow
{
    public string BloodGroup { get; set; } = "";
    public long WbAvailable { get; set; }
    public long WbIssued { get; set; }
    public long WbDiscarded { get; set; }
    public long PcvAvailable { get; set; }
    public long PcvIssued { get; set; }
    public long PcvDiscarded { get; set; }
    public long FfpAvailable { get; set; }
    public long FfpIssued { get; set; }
    public long FfpDiscarded { get; set; }
    public long PcAvailable { get; set; }
    public long PcIssued { get; set; }
    public long PcDiscarded { get; set; }
    public long TotalAvailable { get; set; }
    public long TotalIssued { get; set; }
    public long TotalDiscarded { get; set; }
}

public class DonorListRow
{
    public long DonorId { get; set; }
    public string DonorName { get; set; } = "";
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? LastDonationDate { get; set; }
    public long TotalDonations { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CmIncomeRow
{
    public DateTime InvoiceDate { get; set; }
    public long InvoiceId { get; set; }
    public string PatientName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal EmergencyAmount { get; set; }
    public decimal Discount { get; set; }
}

public class DiscountDetailRow
{
    public long InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string PatientName { get; set; } = "";
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? DiscountReason { get; set; }
    public string? PaymentStatus { get; set; }
}

public class DailyIssueRow
{
    public DateTime IssueDate { get; set; }
    public long InvoiceId { get; set; }
    public string PatientName { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class MbbInwardRow
{
    public long BillId { get; set; }
    public string BillNumber { get; set; } = "";
    public DateTime BillDate { get; set; }
    public string? SupplierName { get; set; }
    public string ComponentType { get; set; } = "";
    public string? BloodGroup { get; set; }
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentStatus { get; set; }
}

public class QcDailyRow
{
    public long QcRecordId { get; set; }
    public string QcType { get; set; } = "";
    public DateTime QcDate { get; set; }
    public long? PerformedBy { get; set; }
    public string? UnitNumber { get; set; }
    public string? Specificity { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? Expiry { get; set; }
    public string? Reactivity { get; set; }
    public string? Activity { get; set; }
    public string? Titre { get; set; }
    public string? Appearance { get; set; }
    public string? Haemolysis { get; set; }
    public string? SpGravity { get; set; }
    public string? HighControl { get; set; }
    public string? LowControl { get; set; }
    public string? Notes { get; set; }
}

public class InvStockRow
{
    public long ItemId { get; set; }
    public string ItemName { get; set; } = "";
    public string? ItemUnit { get; set; }
    public long MinOrderQty { get; set; }
    public long CurrentStock { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

public class InvInOutRow
{
    public long TransId { get; set; }
    public string ItemName { get; set; } = "";
    public long TransQty { get; set; }
    public string TransTyp { get; set; } = "";
    public DateTime TransDate { get; set; }
    public string? TransDesc { get; set; }
    public string? ItemUnit { get; set; }
}

public class InvoiceDetailRow
{
    public long InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string PatientName { get; set; } = "";
    public string? PatientAddress { get; set; }
    public string? PatientContact { get; set; }
    public string? PatientBloodGroup { get; set; }
    public string? HospitalName { get; set; }
    public string? Ward { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMode { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class BsInvoiceDetailRow
{
    public long InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string? StorageName { get; set; }
    public string? StorageAddress { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public string? DonorName { get; set; }
    public DateTime? DonationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long Quantity { get; set; }
    public decimal UnitRate { get; set; }
}

public class CrossMatchReportRow
{
    public long InvoiceId { get; set; }
    public string PatientName { get; set; } = "";
    public string? PatientAddress { get; set; }
    public string? PatientBloodGroup { get; set; }
    public string? HospitalName { get; set; }
    public string? Ward { get; set; }
    public long BloodRequestId { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public string? OverallResult { get; set; }
    public string? TestType { get; set; }
    public string? TestResult { get; set; }
}

public class DiscardRegisterRow
{
    public long DiscardId { get; set; }
    public string? ComponentCode { get; set; }
    public string? ComponentType { get; set; }
    public string? DonorName { get; set; }
    public string? BloodGroup { get; set; }
    public string? DiscardReason { get; set; }
    public DateTime DiscardedAt { get; set; }
    public string? BagNumber { get; set; }
    public DateTime? AutoclaveStart { get; set; }
    public DateTime? AutoclaveEnd { get; set; }
}

public class DuesRegisterRow
{
    public long InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string PatientName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public long DaysOverdue { get; set; }
}
