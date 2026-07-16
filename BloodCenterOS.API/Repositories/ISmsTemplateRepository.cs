using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ISmsTemplateRepository
{
    Task<long> CreateAsync(long centerId, string code, string text);
    Task UpdateAsync(long id, string? code, string? text);
    Task<SmsTemplate?> GetByIdAsync(long id);
    Task<IEnumerable<SmsTemplate>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
