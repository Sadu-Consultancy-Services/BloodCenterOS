namespace BloodCenterOS.API.Repositories;

public interface IQualityControlRepository
{
    Task<long> CreateAsync(long centerId, long deviceId, string detail, long performedBy);
}
