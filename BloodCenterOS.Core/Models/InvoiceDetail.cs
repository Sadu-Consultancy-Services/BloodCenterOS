namespace BloodCenterOS.Core.Models;

public class InvoiceDetail
{
    public long InvoiceDetailId { get; set; }
    public long BillingTransactionId { get; set; }
    public long? ComponentId { get; set; }
    public string? ServiceName { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal? LineTotal { get; set; }
}

public class InvoiceWithDetails
{
    public Billing Invoice { get; set; } = null!;
    public List<InvoiceDetail> Details { get; set; } = new();
}

public class DuesRegisterItem
{
    public long BillingTransactionId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? PatientName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string? PaymentStatus { get; set; }
}

public class CreditNoteRequest
{
    public long OriginalInvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "";
}
