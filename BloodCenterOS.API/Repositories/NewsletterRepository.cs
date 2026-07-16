using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class NewsletterRepository : INewsletterRepository
{
    private readonly IDbConnectionFactory _db;
    public NewsletterRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(long centerId, string email)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_newsletter_create(@p_center_id, @p_email)",
            new { p_center_id = centerId, p_email = email });
    }

    public async Task UpdateAsync(long id, string? email, bool? isActive)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_newsletter_update(@p_subscription_id, @p_email, @p_is_active)",
            new { p_subscription_id = id, p_email = email, p_is_active = isActive });
    }

    public async Task<NewsletterSubscription?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_newsletter_get_by_id(@p_subscription_id)", new { p_subscription_id = id });
        return Map(rows.FirstOrDefault());
    }

    public async Task<IEnumerable<NewsletterSubscription>> GetAllByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_newsletter_get_by_center(@p_center_id)", new { p_center_id = centerId });
        return rows.Select(Map).Where(s => s != null).Select(s => s!);
    }

    public async Task ToggleActiveAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_newsletter_toggle_active(@p_subscription_id)", new { p_subscription_id = id });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("SELECT * FROM fn_newsletter_delete(@p_subscription_id)", new { p_subscription_id = id });
    }

    private static NewsletterSubscription? Map(dynamic r)
    {
        if (r == null) return null;
        return new NewsletterSubscription
        {
            SubscriptionId = (long)r.subscriptionid,
            CenterId = (long?)r.centerid,
            Email = (string?)r.email,
            SubscribedAt = (DateTime?)r.subscribedat,
            IsActive = (bool)r.isactive
        };
    }
}
