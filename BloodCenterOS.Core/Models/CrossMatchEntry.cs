namespace BloodCenterOS.Core.Models;

public class CrossMatchEntry
{
    public long CrossMatchEntryId { get; set; }
    public long CenterId { get; set; }
    public long ReservationId { get; set; }
    public string OverallResult { get; set; } = "Pending";
    public string? Notes { get; set; }
    public long? PerformedBy { get; set; }
    public DateTime? PerformedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? PatientName { get; set; }
    public string? RequiredBloodGroup { get; set; }
    public string? ComponentType { get; set; }
    public int? UnitsReserved { get; set; }
    public string? HospitalName { get; set; }
    public DateTime? ReservationDate { get; set; }
}

public class CrossMatchTestResult
{
    public long TestResultId { get; set; }
    public long CrossMatchEntryId { get; set; }
    public long ReservationDetailId { get; set; }
    public string TestType { get; set; } = "";
    public string Result { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }

    public string? ComponentCode { get; set; }
    public string? BloodGroup { get; set; }
    public string? ComponentType { get; set; }
    public int? VolumeMl { get; set; }
}

public class CrossMatchWithTests
{
    public CrossMatchEntry Entry { get; set; } = null!;
    public List<CrossMatchTestResult> Tests { get; set; } = new();
}

public class StartCrossMatchRequest
{
    public long ReservationId { get; set; }
}

public class SetTestResultRequest
{
    public long TestResultId { get; set; }
    public string Result { get; set; } = "";
}
