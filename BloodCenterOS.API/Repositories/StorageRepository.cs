using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IStorageRepository
{
    Task<IEnumerable<StorageMaster>> GetByCenterAsync(long centerId);
    Task<StorageMaster?> GetByIdAsync(long id);
    Task<long> UpsertAsync(long centerId, StorageMaster item, long userId);
    Task DeleteAsync(long id);
}

public class StorageRepository : IStorageRepository
{
    private readonly string _conn;
    public StorageRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<StorageMaster>> GetByCenterAsync(long centerId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<StorageMaster>("SELECT * FROM fn_storage_get_by_center(@p_center_id)", new { p_center_id = centerId });
    }

    public async Task<StorageMaster?> GetByIdAsync(long id)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryFirstOrDefaultAsync<StorageMaster>("SELECT * FROM fn_storage_get_by_id(@p_id)", new { p_id = id });
    }

    public async Task<long> UpsertAsync(long centerId, StorageMaster item, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_storage_upsert(@p_center_id, @p_id, @p_name, @p_address, @p_phone, @p_email, @p_contact_person, @p_contact_phone, @p_contact_email, @p_rate_wb, @p_rate_pcv, @p_rate_ffp, @p_rate_plts, @p_is_active, @p_created_by)",
            new
            {
                p_center_id = centerId, p_id = item.StorageId > 0 ? item.StorageId : (long?)null,
                p_name = item.StorageName, p_address = item.Address, p_phone = item.PhoneNo,
                p_email = item.Email, p_contact_person = item.ContactPerson,
                p_contact_phone = item.ContactPhone, p_contact_email = item.ContactEmail,
                p_rate_wb = item.RateWB, p_rate_pcv = item.RatePCV, p_rate_ffp = item.RateFFP,
                p_rate_plts = item.RatePltsConc, p_is_active = item.IsActive, p_created_by = userId
            });
    }

    public async Task DeleteAsync(long id)
    {
        using var db = new NpgsqlConnection(_conn);
        await db.ExecuteAsync("SELECT fn_storage_delete(@p_id)", new { p_id = id });
    }
}
