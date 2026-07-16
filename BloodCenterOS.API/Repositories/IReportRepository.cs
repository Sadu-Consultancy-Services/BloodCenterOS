using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<DonorSummaryRow>> GetDonorSummaryAsync(long centerId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<InventorySummaryRow>> GetInventorySummaryAsync(long centerId);
    Task<IEnumerable<CampSummaryRow>> GetCampSummaryAsync(long centerId, DateTime fromDate, DateTime toDate);
}
