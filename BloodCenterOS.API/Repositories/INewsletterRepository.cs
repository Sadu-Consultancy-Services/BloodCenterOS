using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface INewsletterRepository
{
    Task<long> CreateAsync(long centerId, string email);
    Task UpdateAsync(long id, string? email, bool? isActive);
    Task<NewsletterSubscription?> GetByIdAsync(long id);
    Task<IEnumerable<NewsletterSubscription>> GetAllByCenterAsync(long centerId);
    Task ToggleActiveAsync(long id);
    Task DeleteAsync(long id);
}
