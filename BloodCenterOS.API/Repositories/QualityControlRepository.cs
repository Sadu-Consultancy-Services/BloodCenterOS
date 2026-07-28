using Dapper;
using BloodCenterOS.Core.Models;
using Npgsql;

namespace BloodCenterOS.API.Repositories;

public interface IQualityControlRepository
{
    Task<long> CreateAsync(long centerId, CreateQcRequest req, long userId);
    Task<IEnumerable<QualityControl>> GetByCenterAsync(long centerId, string? type, DateTime? from, DateTime? to);
    Task<QualityControl?> GetByIdAsync(long id);
}

public class QualityControlRepository : IQualityControlRepository
{
    private readonly string _conn;
    public QualityControlRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

    public async Task<long> CreateAsync(long centerId, CreateQcRequest req, long userId)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.ExecuteScalarAsync<long>(
            "SELECT fn_qc_create(@p_center_id, @p_qc_type, @p_qc_date::TIMESTAMPTZ, @p_performed_by, " +
            "@p_device_id, @p_unit_number, @p_specificity, @p_batch_no, @p_expiry::DATE, @p_reactivity, " +
            "@p_activity, @p_titre, @p_appearance, @p_haemolysis, @p_sp_gravity, @p_high_control, @p_low_control, @p_notes)",
            new
            {
                p_center_id = centerId,
                p_qc_type = req.QCType,
                p_qc_date = req.QCDate,
                p_performed_by = userId,
                p_device_id = req.DeviceId,
                p_unit_number = req.UnitNumber,
                p_specificity = req.Specificity,
                p_batch_no = req.BatchNo,
                p_expiry = req.Expiry,
                p_reactivity = req.Reactivity,
                p_activity = req.Activity,
                p_titre = req.Titre,
                p_appearance = req.Appearance,
                p_haemolysis = req.Haemolysis,
                p_sp_gravity = req.SpGravity,
                p_high_control = req.HighControl,
                p_low_control = req.LowControl,
                p_notes = req.Notes
            });
    }

    public async Task<IEnumerable<QualityControl>> GetByCenterAsync(long centerId, string? type, DateTime? from, DateTime? to)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryAsync<QualityControl>(
            "SELECT * FROM fn_qc_get_by_center(@p_center_id, @p_qc_type, @p_from_date::DATE, @p_to_date::DATE)",
            new { p_center_id = centerId, p_qc_type = type, p_from_date = from, p_to_date = to });
    }

    public async Task<QualityControl?> GetByIdAsync(long id)
    {
        using var db = new NpgsqlConnection(_conn);
        return await db.QueryFirstOrDefaultAsync<QualityControl>(
            "SELECT * FROM fn_qc_get_by_id(@p_qc_id)",
            new { p_qc_id = id });
    }
}
