namespace BloodCenterOS.Web.Models.ViewModels;

public class ListViewModel<T>
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string CreateUrl { get; set; } = "#";
    public string CreateText { get; set; } = "Add New";
    public List<string> Columns { get; set; } = new();
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ComponentTypeItem
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ShelfLife { get; set; } = "";
}

public class BloodGroupItem
{
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string CanDonateTo { get; set; } = "";
    public string CanReceiveFrom { get; set; } = "";
}

public class DonorListItem
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string BloodGroup { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public DateTime? LastDonation { get; set; }
    public int TotalDonations { get; set; }
    public string Status { get; set; } = "Active";
}
