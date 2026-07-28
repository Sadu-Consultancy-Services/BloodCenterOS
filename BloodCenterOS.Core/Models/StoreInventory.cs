namespace BloodCenterOS.Core.Models;

public class InvItem
{
    public long ItemId { get; set; }
    public long CenterId { get; set; }
    public string ItemName { get; set; } = "";
    public int MinOrderQty { get; set; }
    public string? ItemUnit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class InvTransaction
{
    public long TransId { get; set; }
    public long ItemId { get; set; }
    public string? ItemName { get; set; }
    public int TransQty { get; set; }
    public string TransTyp { get; set; } = "";  // I or O
    public DateTime TransDate { get; set; }
    public string? TransDesc { get; set; }
}

public class InvStockSummary
{
    public long ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemUnit { get; set; }
    public int MinOrderQty { get; set; }
    public long InwardQty { get; set; }
    public long OutwardQty { get; set; }
    public long CurrentStock { get; set; }
}

public class InwardRequest
{
    public long ItemId { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
}

public class OutwardRequest
{
    public long ItemId { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
}
