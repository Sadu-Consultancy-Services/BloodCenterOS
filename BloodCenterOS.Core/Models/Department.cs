namespace BloodCenterOS.Core.Models;

public class Department
{
    public long DepartmentId { get; set; }
    public long? CenterId { get; set; }
    public string? DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}
