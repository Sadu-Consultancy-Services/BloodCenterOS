namespace BloodCenterOS.API.Repositories;

public interface IExpenseRepository
{
    Task<long> CreateAsync(long centerId, string category, decimal amount, string? notes, long createdBy);
}
