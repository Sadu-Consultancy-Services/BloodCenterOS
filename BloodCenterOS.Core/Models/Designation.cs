namespace BloodCenterOS.Core.Models;

public class Designation
{
    public long DesignationId { get; set; }
    public long? CenterId { get; set; }
    public string DesignationName { get; set; } = "";
    public DateTime? CreatedAt { get; set; }
}
