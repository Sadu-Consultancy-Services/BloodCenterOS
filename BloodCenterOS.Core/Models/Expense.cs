namespace BloodCenterOS.Core.Models;

public class Expense
{
    public long ExpenseId { get; set; }
    public long? CenterId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public long? CreatedBy { get; set; }
}
