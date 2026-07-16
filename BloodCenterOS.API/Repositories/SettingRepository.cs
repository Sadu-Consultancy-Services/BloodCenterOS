using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class SettingRepository : ISettingRepository
{
    private readonly IDbConnectionFactory _db;

    public SettingRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<CenterConfigItem>> GetCenterConfigAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CenterConfigItem>(
            "SELECT * FROM fn_center_config_get_all(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task SetCenterConfigAsync(long centerId, string key, string value)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_center_config_set(@p_center_id, @p_key, @p_value)",
            new { p_center_id = centerId, p_key = key, p_value = value });
    }

    public async Task<IEnumerable<SystemConfigItem>> GetSystemConfigAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<SystemConfigItem>(
            "SELECT * FROM fn_system_config_get_all(@p_center_id)",
            new { p_center_id = centerId });
    }

    public async Task SetSystemConfigAsync(long centerId, string key, string value, string? desc)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_config_set(@p_center_id, @p_key, @p_value, @p_desc)",
            new { p_center_id = centerId, p_key = key, p_value = value, p_desc = desc });
    }

    public async Task<IEnumerable<LookupTypeItem>> GetLookupTypesAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<LookupTypeItem>("SELECT * FROM fn_lookup_type_get_all()");
    }

    public async Task<long> CreateLookupTypeAsync(string code, string name, string? desc)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_lookup_type_create(@p_code, @p_name, @p_desc)",
            new { p_code = code, p_name = name, p_desc = desc });
    }

    public async Task<IEnumerable<LookupValueItem>> GetLookupValuesAsync(long typeId, long centerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<LookupValueItem>(
            "SELECT * FROM fn_lookup_value_get_all(@p_type_id, @p_center_id)",
            new { p_type_id = typeId, p_center_id = centerId });
    }

    public async Task<long> CreateLookupValueAsync(long typeId, long centerId, string code, string text, int sort, bool active)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_lookup_value_create(@p_type_id, @p_center_id, @p_code, @p_text, @p_sort, @p_active)",
            new { p_type_id = typeId, p_center_id = centerId, p_code = code, p_text = text, p_sort = sort, p_active = active });
    }
}
