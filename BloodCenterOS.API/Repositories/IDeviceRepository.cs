using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IDeviceRepository
{
    Task<long> CreateAsync(Device device);
    Task UpdateAsync(Device device);
    Task<Device?> GetByIdAsync(long id);
    Task<IEnumerable<Device>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
