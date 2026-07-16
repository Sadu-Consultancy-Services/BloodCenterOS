using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ICampExpenseRepository
{
    Task<long> CreateAsync(long campId, string category, decimal? amount, string? notes);
    Task UpdateAsync(long id, string? category, decimal? amount, string? notes);
    Task<IEnumerable<CampExpense>> GetByCampAsync(long campId);
    Task<IEnumerable<CampExpense>> GetByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
