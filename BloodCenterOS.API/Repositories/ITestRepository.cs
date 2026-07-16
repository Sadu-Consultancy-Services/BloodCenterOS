using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ITestRepository
{
    Task<long> CreateRecordAsync(long centerId, long? collectionId, string? bagNumber, long performedBy);
    Task<BloodTestRecord?> GetRecordByIdAsync(long id);
    Task<IEnumerable<BloodTestRecord>> GetPendingAsync(long centerId);
    Task<long> AddResultAsync(BloodTestResult result);
    Task<IEnumerable<BloodTestResult>> GetResultsByRecordAsync(long testRecordId);
    Task UpdateRecordStatusAsync(long recordId, string status);
}
