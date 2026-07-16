namespace BloodCenterOS.Core.Models;

public class Billing
{
    public long BillingTransactionId { get; set; }
    public long? CenterId { get; set; }
    public string? InvoiceNumber { get; set; }
    public long? PatientId { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? Discount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMode { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
