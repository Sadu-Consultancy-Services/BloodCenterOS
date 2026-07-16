using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IBillingRepository
{
    Task<long> CreateBillingAsync(Billing billing);
    Task<long> AddPaymentAsync(long billingId, long centerId, decimal amount, string mode, string? reference, long? createdBy);
    Task<IEnumerable<Billing>> GetByCenterAsync(long centerId);
}
