namespace BloodCenterOS.Core.Models;

public class CampExpense
{
    public long CampExpenseId { get; set; }
    public long CampId { get; set; }
    public string? CampName { get; set; }
    public string? ExpenseCategory { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
}
