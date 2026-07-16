using BloodCenterOS.Core.Models;

namespace BloodCenterOS.Web.Models.ViewModels;

public class DonorViewModel
{
    public Donor Donor { get; set; } = new();
    public DonorDetailViewModel? Detail { get; set; }
}

public class DonorDetailViewModel
{
    public Donor Donor { get; set; } = new();
    public List<Donation> Donations { get; set; } = new();
}

public class DonorSearchViewModel
{
    public string? Keyword { get; set; }
    public string? BloodGroup { get; set; }
    public string? Gender { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<DonorListItem> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
}
