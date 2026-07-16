namespace BloodCenterOS.API.Repositories;

public interface ICrossMatchRepository
{
    Task<long> CreateAsync(long centerId, long requestId, long componentId, string? result, string? method, long performedBy);
}
