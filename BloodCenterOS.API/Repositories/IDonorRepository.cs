using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IDonorRepository
{
    Task<long> CreateAsync(Donor donor);
    Task<Donor?> GetByIdAsync(long id);
    Task UpdateAsync(Donor donor);
    Task<PagedResult<Donor>> SearchAsync(long? centerId, string? keyword, string? bloodGroup, string? gender, int page, int size);
    Task<IEnumerable<Donor>> GetByPhoneAsync(long centerId, string phone);
    Task<IEnumerable<Donation>> GetDonationHistoryByDonorAsync(long donorId);
}
