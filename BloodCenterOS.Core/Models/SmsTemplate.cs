namespace BloodCenterOS.Core.Models;

public class SmsTemplate
{
    public long SmsTemplateId { get; set; }
    public long? CenterId { get; set; }
    public string? TemplateCode { get; set; }
    public string? TemplateText { get; set; }
    public DateTime? CreatedAt { get; set; }
}
