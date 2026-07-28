using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IMbbBillRepository
{
    Task<long> CreateBillAsync(long centerId, CreateMbbBillRequest request, long userId);
    Task<long> AddDetailAsync(long billId, string componentType, string? bloodGroup, int quantity, decimal unitPrice, string? bagNumbers);
    Task<IEnumerable<MbbBill>> GetByCenterAsync(long centerId);
    Task<MbbBill?> GetByIdAsync(long billId);
    Task<IEnumerable<MbbBillDetail>> GetDetailAsync(long billId);
    Task MakePaymentAsync(long billId, decimal amount, string paymentMode, long userId);
}

public class MbbBillRepository : IMbbBillRepository
{
    private readonly string _conn;
    public MbbBillRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<long> CreateBillAsync(long centerId, CreateMbbBillRequest request, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        var billId = await db.ExecuteScalarAsync<long>(
            "SELECT fn_mbb_bill_create(@p_center_id, @p_bill_number, @p_bill_date::TIMESTAMPTZ, @p_supplier_name, @p_payment_mode, @p_cheque_no, @p_cheque_date::DATE, @p_notes, @p_created_by)",
            new
            {
                p_center_id = centerId,
                p_bill_number = request.BillNumber,
                p_bill_date = request.BillDate,
                p_supplier_name = request.SupplierName,
                p_payment_mode = request.PaymentMode,
                p_cheque_no = request.ChequeNo,
                p_cheque_date = request.ChequeDate,
                p_notes = request.Notes,
                p_created_by = userId
            });

        foreach (var detail in request.Details)
        {
            await AddDetailAsync(billId, detail.ComponentType, detail.BloodGroup, detail.Quantity, detail.UnitPrice, detail.BagNumbers);
        }

        return billId;
    }

    public async Task<long> AddDetailAsync(long billId, string componentType, string? bloodGroup, int quantity, decimal unitPrice, string? bagNumbers)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_mbb_bill_add_detail(@p_mbb_bill_id, @p_component_type, @p_blood_group, @p_quantity, @p_unit_price, @p_bag_numbers)",
            new { p_mbb_bill_id = billId, p_component_type = componentType, p_blood_group = bloodGroup, p_quantity = quantity, p_unit_price = unitPrice, p_bag_numbers = bagNumbers });
    }

    public async Task<IEnumerable<MbbBill>> GetByCenterAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<MbbBill>(
            "SELECT * FROM fn_mbb_bill_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task<MbbBill?> GetByIdAsync(long billId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryFirstOrDefaultAsync<MbbBill>(
            "SELECT * FROM fn_mbb_bill_get_by_id(@p_bill_id)",
            new { p_bill_id = billId });
    }

    public async Task<IEnumerable<MbbBillDetail>> GetDetailAsync(long billId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<MbbBillDetail>(
            "SELECT * FROM fn_mbb_bill_get_detail(@p_bill_id)",
            new { p_bill_id = billId });
    }

    public async Task MakePaymentAsync(long billId, decimal amount, string paymentMode, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        await db.ExecuteAsync("SELECT fn_mbb_bill_make_payment(@p_bill_id, @p_amount, @p_payment_mode, @p_created_by)",
            new { p_bill_id = billId, p_amount = amount, p_payment_mode = paymentMode, p_created_by = userId });
    }
}
