namespace BloodCenterOS.Core.Models;

public class RateMaster
{
    public long RateId { get; set; }
    public long CenterId { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal UnitRate { get; set; }
    public decimal ReservationRate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RateUpsertRequest
{
    public string BloodGroup { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal UnitRate { get; set; }
    public decimal ReservationRate { get; set; }
}
