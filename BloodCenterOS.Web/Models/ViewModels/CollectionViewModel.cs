using BloodCenterOS.Core.Models;

namespace BloodCenterOS.Web.Models.ViewModels;

public class CollectionListViewModel
{
    public List<Collection> Collections { get; set; } = new();
}

public class CollectionCreateViewModel
{
    public Collection Collection { get; set; } = new();
    public List<DonorListItem> Donors { get; set; } = new();
    public List<Camp> Camps { get; set; } = new();
}
