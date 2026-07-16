using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class ComponentTypeRepository : IComponentTypeRepository
{
    private readonly IDbConnectionFactory _db;
    public ComponentTypeRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(string code, string? desc)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_component_type_create(@p_code, @p_desc)", new { p_code = code, p_desc = desc });
    }

    public async Task UpdateAsync(long id, string? code, string? desc)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_component_type_update(@p_id, @p_code, @p_desc)", new { p_id = id, p_code = code, p_desc = desc });
    }

    public async Task<IEnumerable<ComponentType>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_component_type_get_all()");
        return rows.Select(Map).Where(t => t != null).Select(t => t!);
    }

    public async Task<ComponentType?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_component_type_get_by_id(@p_id)", new { p_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_component_type_delete(@p_id)", new { p_id = id });
    }

    private static ComponentType? Map(dynamic? r)
    {
        if (r == null) return null;
        return new ComponentType
        {
            ComponentTypeId = (long)r.componenttypeid,
            ComponentTypeCode = (string?)r.componenttypecode,
            Description = (string?)r.description,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
