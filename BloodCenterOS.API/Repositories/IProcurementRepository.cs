using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IProcurementRepository
{
    Task<IEnumerable<ProcurementRegisterItem>> SearchAsync(
        long centerId, string? bloodGroup, string? componentType,
        string? status, DateTime? fromDate, DateTime? toDate, string? keyword);

    Task<IEnumerable<ProcurementRegisterSummaryRow>> GetSummaryAsync(long centerId);
}
