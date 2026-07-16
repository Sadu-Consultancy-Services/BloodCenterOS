using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly IDbConnectionFactory _db;

    public PermissionRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Permission>("SELECT * FROM fn_permission_get_all()");
    }
}
