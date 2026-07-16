namespace BloodCenterOS.API.Repositories;

public interface IReplacementDonorRepository
{
    Task<long> RegisterAsync(long centerId, long requestId, long donorId);
}
