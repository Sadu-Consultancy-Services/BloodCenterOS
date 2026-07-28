using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<DonorSummaryRow>> GetDonorSummaryAsync(long centerId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<InventorySummaryRow>> GetInventorySummaryAsync(long centerId);
    Task<IEnumerable<CampSummaryRow>> GetCampSummaryAsync(long centerId, DateTime fromDate, DateTime toDate);

    // Phase 9 reports
    Task<IEnumerable<BloodStockRow>> GetBloodStockAsync(long centerId);
    Task<IEnumerable<ProcurementSummaryRow>> GetProcurementSummaryAsync(long centerId, DateTime from, DateTime to);
    Task<IEnumerable<DonorListRow>> GetDonorListAsync(long centerId, DateTime from, DateTime to, bool showContact);
    Task<IEnumerable<CmIncomeRow>> GetCmIncomeAsync(long centerId, DateTime from, DateTime to);
    Task<IEnumerable<DiscountDetailRow>> GetDiscountDetailsAsync(long centerId, DateTime from, DateTime to);
    Task<IEnumerable<DailyIssueRow>> GetDailyIssuesAsync(long centerId, DateTime from, DateTime to);
    Task<IEnumerable<MbbInwardRow>> GetMbbInwardAsync(long centerId, DateTime from, DateTime to, string? supplier);
    Task<IEnumerable<QcDailyRow>> GetQcDailyAsync(long centerId, DateTime date);
    Task<IEnumerable<InvStockRow>> GetInvStockAsync(long centerId);
    Task<IEnumerable<InvInOutRow>> GetInvInOutAsync(long centerId, DateTime from, DateTime to, string? type, long[]? itemIds);
    Task<IEnumerable<InvoiceDetailRow>> GetInvoiceDetailAsync(long centerId, long invoiceId);
    Task<IEnumerable<BsInvoiceDetailRow>> GetBsInvoiceDetailAsync(long centerId, long invoiceId);
    Task<IEnumerable<CrossMatchReportRow>> GetCrossMatchReportAsync(long centerId, long invoiceId);
    Task<IEnumerable<DiscardRegisterRow>> GetDiscardRegisterAsync(long centerId, DateTime from, DateTime to, string? reason);
    Task<IEnumerable<DuesRegisterRow>> GetDuesRegisterAsync(long centerId, DateTime? asOnDate);
    Task<IEnumerable<DiscardRegisterRow>> GetAutoclaveRegisterAsync(long centerId, DateTime from, DateTime to);
}
