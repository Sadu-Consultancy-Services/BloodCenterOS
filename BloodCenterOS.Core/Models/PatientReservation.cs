namespace BloodCenterOS.Core.Models;

public class PatientReservation
{
    public long ReservationId { get; set; }
    public long CenterId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientAddress { get; set; }
    public string? PatientContactNo { get; set; }
    public string PatientBloodGroup { get; set; } = string.Empty;
    public string RequiredBloodGroup { get; set; } = string.Empty;
    public string? HospitalName { get; set; }
    public string? Ward { get; set; }
    public string ComponentType { get; set; } = string.Empty;
    public int UnitsRequested { get; set; }
    public int UnitsReserved { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime ReservationDate { get; set; }
    public long? InvoiceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public List<ReservationDetail> Details { get; set; } = new();
}

public class ReservationDetail
{
    public long ReservationDetailId { get; set; }
    public long ReservationId { get; set; }
    public long ComponentId { get; set; }
    public string? ComponentCode { get; set; }
    public string? BloodGroup { get; set; }
    public string? ComponentType { get; set; }
    public int? VolumeMl { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal UnitRate { get; set; }
    public decimal ReservationRate { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReservationCreateRequest
{
    public string PatientName { get; set; } = string.Empty;
    public string? PatientAddress { get; set; }
    public string? PatientContactNo { get; set; }
    public string PatientBloodGroup { get; set; } = string.Empty;
    public string RequiredBloodGroup { get; set; } = string.Empty;
    public string? HospitalName { get; set; }
    public string? Ward { get; set; }
    public string ComponentType { get; set; } = string.Empty;
    public int Units { get; set; } = 1;
    public bool CreateInvoice { get; set; }
    public string? Notes { get; set; }
}

public class AvailableComponentItem
{
    public long ComponentId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string? ComponentType { get; set; }
    public int? VolumeMl { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? StorageLocation { get; set; }
    public decimal UnitRate { get; set; }
    public decimal ReservationRate { get; set; }
}
