namespace BloodCenterOS.Core.Models;

public class DonorSummaryRow
{
    public string Period { get; set; } = "";
    public long TotalRegistered { get; set; }
    public long TotalBloodGroupAPositive { get; set; }
    public long TotalBloodGroupANegative { get; set; }
    public long TotalBloodGroupBPositive { get; set; }
    public long TotalBloodGroupBNegative { get; set; }
    public long TotalBloodGroupAbPositive { get; set; }
    public long TotalBloodGroupAbNegative { get; set; }
    public long TotalBloodGroupOPositive { get; set; }
    public long TotalBloodGroupONegative { get; set; }
    public long TotalDeferrals { get; set; }
    public long TotalCollections { get; set; }
}

public class InventorySummaryRow
{
    public string ComponentType { get; set; } = "";
    public string BloodGroup { get; set; } = "";
    public long AvailableQty { get; set; }
    public long ReservedQty { get; set; }
    public long QuarantinedQty { get; set; }
    public long NearExpiryQty { get; set; }
}

public class CampSummaryRow
{
    public string Period { get; set; } = "";
    public long TotalCamps { get; set; }
    public long TotalExpected { get; set; }
    public long TotalCollected { get; set; }
    public decimal CollectionRate { get; set; }
}

public class CenterConfigItem
{
    public string ConfigKey { get; set; } = "";
    public string? ConfigValue { get; set; }
}

public class SystemConfigItem
{
    public string ConfigKey { get; set; } = "";
    public string? ConfigValue { get; set; }
    public string? Description { get; set; }
}

public class LookupTypeItem
{
    public long LookupTypeId { get; set; }
    public string TypeCode { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
}

public class LookupValueItem
{
    public long LookupValueId { get; set; }
    public long? LookupTypeId { get; set; }
    public string ValueCode { get; set; } = "";
    public string ValueText { get; set; } = "";
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}
