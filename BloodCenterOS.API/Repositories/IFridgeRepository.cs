using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IFridgeRepository
{
    Task<long> CreateAsync(Fridge fridge);
    Task UpdateAsync(Fridge fridge);
    Task<Fridge?> GetByIdAsync(long id);
    Task<IEnumerable<Fridge>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
