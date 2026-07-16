using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class BillingRepository : IBillingRepository
{
    private readonly IDbConnectionFactory _db;

    public BillingRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateBillingAsync(Billing billing)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_billing_create(@p_center_id, @p_invoice_no, @p_patient_id, @p_total, @p_tax, @p_discount, @p_payment_status, @p_payment_mode, @p_created_by)",
            new
            {
                p_center_id = billing.CenterId,
                p_invoice_no = billing.InvoiceNumber,
                p_patient_id = billing.PatientId,
                p_total = billing.TotalAmount,
                p_tax = billing.TaxAmount,
                p_discount = billing.Discount,
                p_payment_status = billing.PaymentStatus,
                p_payment_mode = billing.PaymentMode,
                p_created_by = billing.CreatedBy
            });
    }

    public async Task<IEnumerable<Billing>> GetByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_billing_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new Billing
        {
            BillingTransactionId = (long)r.billingtransactionid,
            CenterId = (long?)r.centerid,
            InvoiceNumber = (string?)r.invoicenumber,
            PatientId = (long?)r.patientid,
            TotalAmount = (decimal?)r.totalamount,
            TaxAmount = (decimal?)r.taxamount,
            Discount = (decimal?)r.discount,
            PaymentStatus = (string?)r.paymentstatus,
            PaymentMode = (string?)r.paymentmode,
            InvoiceDate = (DateTime)r.invoicedate,
            CreatedAt = (DateTime)r.createdat
        });
    }

    public async Task<long> AddPaymentAsync(long billingId, long centerId, decimal amount, string mode, string? reference, long? createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_payment_create(@p_billing_id, @p_center_id, @p_amount, @p_mode, @p_reference, @p_created_by)",
            new
            {
                p_billing_id = billingId,
                p_center_id = centerId,
                p_amount = amount,
                p_mode = mode,
                p_reference = reference,
                p_created_by = createdBy
            });
    }
}
