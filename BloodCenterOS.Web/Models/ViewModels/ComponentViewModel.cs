using BloodCenterOS.Core.Models;

namespace BloodCenterOS.Web.Models.ViewModels;

public class ComponentListViewModel
{
    public List<Component> AvailableComponents { get; set; } = new();
    public string? BloodGroupFilter { get; set; }
}
