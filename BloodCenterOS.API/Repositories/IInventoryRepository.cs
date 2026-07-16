using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IInventoryRepository
{
    Task<long> UpsertAsync(long centerId, string? componentType, string? bloodGroup, int available, int reserved, int quarantined, long? updatedBy);
    Task<IEnumerable<InventoryStock>> GetStockAsync(long centerId);
    Task<IEnumerable<dynamic>> GetSummaryAsync(long centerId);
}
