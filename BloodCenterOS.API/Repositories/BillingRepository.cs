using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IBillingRepository
{
    Task<long> CreateBillingAsync(Billing billing);
    Task<long> AddPaymentAsync(long billingId, long centerId, decimal amount, string mode, string? reference, long? createdBy);
    Task<IEnumerable<Billing>> GetByCenterAsync(long centerId);
    Task<Billing?> GetByIdAsync(long billingId);
    Task<IEnumerable<InvoiceDetail>> GetDetailAsync(long billingId);
    Task<IEnumerable<DuesRegisterItem>> GetDuesAsync(long centerId, string? keyword);
    Task<long> CreateCreditNoteAsync(long centerId, long originalInvoiceId, decimal amount, string reason, long createdBy);
}

public class BillingRepository : IBillingRepository
{
    private readonly string _conn;
    public BillingRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<long> CreateBillingAsync(Billing billing)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_billing_create(@p_center_id, @p_invoice_no, @p_patient_id, @p_total, @p_tax, @p_discount, @p_payment_status, @p_payment_mode, @p_created_by)",
            new
            {
                p_center_id = billing.CenterId,
                p_invoice_no = billing.InvoiceNumber,
                p_patient_id = billing.PatientId,
                p_total = billing.TotalAmount,
                p_tax = billing.TaxAmount ?? 0,
                p_discount = billing.Discount ?? 0,
                p_payment_status = billing.PaymentStatus,
                p_payment_mode = billing.PaymentMode,
                p_created_by = billing.CreatedBy
            });
    }

    public async Task<IEnumerable<Billing>> GetByCenterAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<Billing>(
            "SELECT * FROM fn_billing_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task<Billing?> GetByIdAsync(long billingId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryFirstOrDefaultAsync<Billing>(
            "SELECT * FROM fn_billing_get_by_id(@p_billing_id)",
            new { p_billing_id = billingId });
    }

    public async Task<IEnumerable<InvoiceDetail>> GetDetailAsync(long billingId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<InvoiceDetail>(
            "SELECT * FROM fn_billing_get_detail(@p_billing_id)",
            new { p_billing_id = billingId });
    }

    public async Task<IEnumerable<DuesRegisterItem>> GetDuesAsync(long centerId, string? keyword)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<DuesRegisterItem>(
            "SELECT * FROM fn_billing_get_dues(@p_center_id, @p_patient_name)",
            new { p_center_id = centerId, p_patient_name = keyword });
    }

    public async Task<long> AddPaymentAsync(long billingId, long centerId, decimal amount, string mode, string? reference, long? createdBy)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_payment_create(@p_billing_id, @p_center_id, @p_amount, @p_mode, @p_reference, @p_created_by)",
            new { p_billing_id = billingId, p_center_id = centerId, p_amount = amount, p_mode = mode, p_reference = reference, p_created_by = createdBy });
    }

    public async Task<long> CreateCreditNoteAsync(long centerId, long originalInvoiceId, decimal amount, string reason, long createdBy)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_billing_credit_note(@p_center_id, @p_original_invoice_id, @p_amount, @p_reason, @p_created_by)",
            new { p_center_id = centerId, p_original_invoice_id = originalInvoiceId, p_amount = amount, p_reason = reason, p_created_by = createdBy });
    }
}
