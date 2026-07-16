namespace BloodCenterOS.Core.Models;

public class EmailTemplate
{
    public long EmailTemplateId { get; set; }
    public long? CenterId { get; set; }
    public string? TemplateCode { get; set; }
    public string? Subject { get; set; }
    public string? BodyHtml { get; set; }
    public DateTime? CreatedAt { get; set; }
}
