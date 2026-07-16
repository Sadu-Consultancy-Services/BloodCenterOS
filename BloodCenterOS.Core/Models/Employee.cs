namespace BloodCenterOS.Core.Models;

public class Employee
{
    public long EmployeeId { get; set; }
    public long? CenterId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Designation { get; set; }
    public long? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateOnly? JoinDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
