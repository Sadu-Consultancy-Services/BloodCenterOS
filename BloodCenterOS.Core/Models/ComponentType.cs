namespace BloodCenterOS.Core.Models;

public class ComponentType
{
    public long ComponentTypeId { get; set; }
    public string? ComponentTypeCode { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}
