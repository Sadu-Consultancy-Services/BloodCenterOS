namespace BloodCenterOS.API.Repositories;

public interface IReturnRepository
{
    Task<long> CreateAsync(long centerId, long issueId, long componentId, string reason, long createdBy);
}
