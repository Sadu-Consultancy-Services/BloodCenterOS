using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IComponentRepository
{
    Task<long> PrepareAsync(long centerId, long bagId, string componentType, int volume, long preparedBy);
    Task<IEnumerable<Component>> GetAvailableAsync(long centerId, string? bloodGroup);
    Task<long> TransferAsync(long centerId, long componentId, long toCenterId, string? transportDetails, long createdBy);
    Task<long> DiscardAsync(long centerId, long? bagId, long? componentId, string reason, long discardedBy, string? notes);
}
