using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IExpenseRepository
{
    Task<long> CreateAsync(long centerId, string category, decimal amount, string? notes, long createdBy);
    Task<IEnumerable<Expense>> GetAllAsync(long centerId, DateTime? from, DateTime? to);
}
