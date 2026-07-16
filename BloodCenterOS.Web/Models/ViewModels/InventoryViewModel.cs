using BloodCenterOS.Core.Models;

namespace BloodCenterOS.Web.Models.ViewModels;

public class InventoryViewModel
{
    public List<InventoryStock> Stock { get; set; } = new();
    public int TotalAvailable { get; set; }
    public int TotalReserved { get; set; }
    public int TotalQuarantined { get; set; }
}
