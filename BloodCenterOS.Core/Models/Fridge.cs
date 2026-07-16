namespace BloodCenterOS.Core.Models;

public class Fridge
{
    public long FridgeId { get; set; }
    public long? CenterId { get; set; }
    public string? FridgeCode { get; set; }
    public string? FridgeName { get; set; }
    public int? Capacity { get; set; }
    public string? Location { get; set; }
    public bool TemperatureLogRequired { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
}
