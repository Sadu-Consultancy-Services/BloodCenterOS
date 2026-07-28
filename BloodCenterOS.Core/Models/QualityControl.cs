namespace BloodCenterOS.Core.Models;

public class QualityControl
{
    public long QCRecordId { get; set; }
    public long? CenterId { get; set; }
    public string QCType { get; set; } = "";
    public DateTime QCDate { get; set; }
    public long? PerformedBy { get; set; }
    public long? DeviceId { get; set; }

    public string? UnitNumber { get; set; }
    public string? Specificity { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? Expiry { get; set; }
    public string? Reactivity { get; set; }
    public string? Activity { get; set; }
    public string? Titre { get; set; }
    public string? Appearance { get; set; }
    public string? Haemolysis { get; set; }
    public string? SpGravity { get; set; }
    public string? HighControl { get; set; }
    public string? LowControl { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateQcRequest
{
    public string QCType { get; set; } = "";
    public DateTime QCDate { get; set; } = DateTime.Today;
    public long? DeviceId { get; set; }

    public string? UnitNumber { get; set; }
    public string? Specificity { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? Expiry { get; set; }
    public string? Reactivity { get; set; }
    public string? Activity { get; set; }
    public string? Titre { get; set; }
    public string? Appearance { get; set; }
    public string? Haemolysis { get; set; }
    public string? SpGravity { get; set; }
    public string? HighControl { get; set; }
    public string? LowControl { get; set; }
    public string? Notes { get; set; }
}
