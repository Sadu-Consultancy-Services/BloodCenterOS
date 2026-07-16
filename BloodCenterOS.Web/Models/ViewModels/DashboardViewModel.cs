namespace BloodCenterOS.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalDonors { get; set; } = 1250;
    public int TodayCollections { get; set; } = 8;
    public int PendingTests { get; set; } = 12;
    public int PendingRequests { get; set; } = 5;
    public int AvailableUnits { get; set; } = 142;
    public int ExpiringUnits { get; set; } = 3;
    public List<StockItem> StockSummary { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<AlertItem> Alerts { get; set; } = new();
}

public class StockItem
{
    public string BloodGroup { get; set; } = "";
    public int Available { get; set; }
    public int Reserved { get; set; }
    public int Quarantined { get; set; }
}

public class RecentActivity
{
    public string Time { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "info";
}

public class AlertItem
{
    public string Type { get; set; } = "warning";
    public string Message { get; set; } = "";
}
