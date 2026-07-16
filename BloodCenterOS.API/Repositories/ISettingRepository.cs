using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ISettingRepository
{
    Task<IEnumerable<CenterConfigItem>> GetCenterConfigAsync(long centerId);
    Task SetCenterConfigAsync(long centerId, string key, string value);
    Task<IEnumerable<SystemConfigItem>> GetSystemConfigAsync(long centerId);
    Task SetSystemConfigAsync(long centerId, string key, string value, string? desc);
    Task<IEnumerable<LookupTypeItem>> GetLookupTypesAsync();
    Task<long> CreateLookupTypeAsync(string code, string name, string? desc);
    Task<IEnumerable<LookupValueItem>> GetLookupValuesAsync(long typeId, long centerId);
    Task<long> CreateLookupValueAsync(long typeId, long centerId, string code, string text, int sort, bool active);
}
