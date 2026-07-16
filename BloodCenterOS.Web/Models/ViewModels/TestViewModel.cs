using BloodCenterOS.Core.Models;

namespace BloodCenterOS.Web.Models.ViewModels;

public class TestListViewModel
{
    public List<BloodTestRecord> PendingTests { get; set; } = new();
    public List<BloodTestRecord> CompletedTests { get; set; } = new();
}

public class TestDetailViewModel
{
    public BloodTestRecord Record { get; set; } = new();
    public List<BloodTestResult> Results { get; set; } = new();
}
