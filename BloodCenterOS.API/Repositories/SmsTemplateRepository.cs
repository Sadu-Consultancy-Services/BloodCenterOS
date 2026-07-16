using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class SmsTemplateRepository : ISmsTemplateRepository
{
    private readonly IDbConnectionFactory _db;
    public SmsTemplateRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, string code, string text)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_sms_template_create(@p_center_id, @p_code, @p_text)",
            new { p_center_id = centerId, p_code = code, p_text = text });
    }

    public async Task UpdateAsync(long id, string? code, string? text)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_sms_template_update(@p_template_id, @p_code, @p_text)",
            new { p_template_id = id, p_code = code, p_text = text });
    }

    public async Task<SmsTemplate?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_sms_template_get_by_id(@p_template_id)", new { p_template_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<SmsTemplate>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_sms_template_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(t => t != null).Select(t => t!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_sms_template_delete(@p_template_id)", new { p_template_id = id });
    }

    private static SmsTemplate? Map(dynamic r)
    {
        if (r == null) return null;
        return new SmsTemplate
        {
            SmsTemplateId = (long)r.smstemplateid,
            CenterId = (long?)r.centerid,
            TemplateCode = (string?)r.templatecode,
            TemplateText = (string?)r.templatetext,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
