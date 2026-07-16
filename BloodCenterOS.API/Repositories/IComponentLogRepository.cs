namespace BloodCenterOS.API.Repositories;

public interface IComponentLogRepository
{
    Task<long> StoreAsync(long centerId, long componentId, long fridgeId, string? location, string? notes);
    Task<long> TransferAsync(long centerId, long componentId, long toCenterId, string? transportDetails, long createdBy);
    Task<long> DiscardAsync(long centerId, long bagId, long componentId, string reason, long discardedBy, string? notes);
    Task UpdateStatusAsync(long componentId, string status);
}
