using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface IIssueRepository
{
    Task<long> CreateIssueAsync(IssueRecord issue);
    Task<IEnumerable<PatientRequest>> GetPendingRequestsAsync(long centerId);
    Task<IEnumerable<IssueRecord>> GetByCenterAsync(long centerId);
}
