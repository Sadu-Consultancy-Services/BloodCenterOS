using BloodCenterOS.Core.Models;

namespace BloodCenterOS.Web.Models.ViewModels;

public class CampListViewModel
{
    public List<Camp> UpcomingCamps { get; set; } = new();
    public List<Camp> PastCamps { get; set; } = new();
}

public class CampDetailViewModel
{
    public Camp Camp { get; set; } = new();
    public int RegisteredDonors { get; set; }
    public int CollectedUnits { get; set; }
}
