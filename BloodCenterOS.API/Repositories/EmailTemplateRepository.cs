using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly IDbConnectionFactory _db;
    public EmailTemplateRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, string code, string subject, string body)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_email_template_create(@p_center_id, @p_code, @p_subject, @p_body)",
            new { p_center_id = centerId, p_code = code, p_subject = subject, p_body = body });
    }

    public async Task UpdateAsync(long id, string? code, string? subject, string? body)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_email_template_update(@p_template_id, @p_code, @p_subject, @p_body)",
            new { p_template_id = id, p_code = code, p_subject = subject, p_body = body });
    }

    public async Task<EmailTemplate?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_email_template_get_by_id(@p_template_id)", new { p_template_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_email_template_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(t => t != null).Select(t => t!);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_email_template_delete(@p_template_id)", new { p_template_id = id });
    }

    private static EmailTemplate? Map(dynamic r)
    {
        if (r == null) return null;
        return new EmailTemplate
        {
            EmailTemplateId = (long)r.emailtemplateid,
            CenterId = (long?)r.centerid,
            TemplateCode = (string?)r.templatecode,
            Subject = (string?)r.subject,
            BodyHtml = (string?)r.bodyhtml,
            CreatedAt = (DateTime?)r.createdat
        };
    }
}
