using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ICampInventoryRepository
{
    Task<long> CreateAsync(long campId, string itemName, int? quantity, string? unit);
    Task UpdateAsync(long id, string? itemName, int? quantity, string? unit);
    Task<IEnumerable<CampInventory>> GetByCampAsync(long campId);
    Task<IEnumerable<CampInventory>> GetByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
