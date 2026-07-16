using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IEmailTemplateRepository
{
    Task<long> CreateAsync(long centerId, string code, string subject, string body);
    Task UpdateAsync(long id, string? code, string? subject, string? body);
    Task<EmailTemplate?> GetByIdAsync(long id);
    Task<IEnumerable<EmailTemplate>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
