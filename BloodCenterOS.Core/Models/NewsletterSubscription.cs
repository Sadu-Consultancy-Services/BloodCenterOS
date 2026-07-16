namespace BloodCenterOS.Core.Models;

public class NewsletterSubscription
{
    public long SubscriptionId { get; set; }
    public long? CenterId { get; set; }
    public string? Email { get; set; }
    public DateTime? SubscribedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
