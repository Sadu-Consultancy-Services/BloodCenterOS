namespace BloodCenterOS.Core.Models;

public class MbbBill
{
    public long MbbBillId { get; set; }
    public long CenterId { get; set; }
    public string? BillNumber { get; set; }
    public DateTime BillDate { get; set; }
    public string? SupplierName { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? PaymentMode { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? Notes { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MbbBillDetail
{
    public long MbbBillDetailId { get; set; }
    public long MbbBillId { get; set; }
    public string? ComponentType { get; set; }
    public string? BloodGroup { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal? LineTotal { get; set; }
    public string? BagNumbers { get; set; }
}

public class MbbBillWithDetails
{
    public MbbBill Bill { get; set; } = null!;
    public List<MbbBillDetail> Details { get; set; } = new();
}

public class CreateMbbBillRequest
{
    public string BillNumber { get; set; } = "";
    public DateTime BillDate { get; set; } = DateTime.Today;
    public string? SupplierName { get; set; }
    public string? PaymentMode { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? Notes { get; set; }
    public List<MbbBillDetailRequest> Details { get; set; } = new();
}

public class MbbBillDetailRequest
{
    public string ComponentType { get; set; } = "";
    public string? BloodGroup { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string? BagNumbers { get; set; }
}
