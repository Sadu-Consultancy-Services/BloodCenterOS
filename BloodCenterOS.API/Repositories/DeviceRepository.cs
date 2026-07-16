using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly IDbConnectionFactory _db;
    public DeviceRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Device device)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_device_create(@p_center_id, @p_name, @p_type, @p_serial, @p_purchase_date, @p_warranty_end)",
            new { p_center_id = device.CenterId, p_name = device.DeviceName, p_type = device.DeviceType, p_serial = device.SerialNumber, p_purchase_date = device.PurchaseDate?.ToDateTime(TimeOnly.MinValue), p_warranty_end = device.WarrantyEndDate?.ToDateTime(TimeOnly.MinValue) });
    }

    public async Task UpdateAsync(Device device)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_device_update(@p_device_id, @p_name, @p_type, @p_serial, @p_purchase_date, @p_warranty_end)",
            new { p_device_id = device.DeviceId, p_name = device.DeviceName, p_type = device.DeviceType, p_serial = device.SerialNumber, p_purchase_date = device.PurchaseDate?.ToDateTime(TimeOnly.MinValue), p_warranty_end = device.WarrantyEndDate?.ToDateTime(TimeOnly.MinValue) });
    }

    public async Task<Device?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_device_get_by_id(@p_device_id)", new { p_device_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<Device>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM fn_device_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(d => d != null).Select(d => d!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_device_delete(@p_device_id)", new { p_device_id = id });
    }

    private static Device? Map(dynamic r)
    {
        if (r == null) return null;
        return new Device
        {
            DeviceId = (long)r.deviceid,
            CenterId = (long?)r.centerid,
            DeviceName = (string?)r.devicename,
            DeviceType = (string?)r.devicetype,
            SerialNumber = (string?)r.serialnumber,
            PurchaseDate = r.purchasedate != null ? DateOnly.FromDateTime((DateTime)r.purchasedate) : null,
            WarrantyEndDate = r.warrantyenddate != null ? DateOnly.FromDateTime((DateTime)r.warrantyenddate) : null,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
