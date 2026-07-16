namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class InventoryControllerTests : IntegrationTestBase
{
    public InventoryControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AdjustInventory_WithAuth_ReturnsId()
    {
        var token = await GetTokenAsync();
        var url = "/api/inventory/adjust?componentType=PRBC&bloodGroup=O%2B&available=50&reserved=5&quarantined=2";

        var result = await PostAsync<long>(url, null, token);

        Assert.True(result.Success);
        Assert.True(result.Data > 0);
    }

    [Fact]
    public async Task GetStock_ReturnsInventoryList()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<List<InventoryStock>>("/api/inventory/stock", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetSummary_ReturnsSummaryList()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<List<object>>("/api/inventory/summary", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    public class InventoryStock
    {
        public long InventoryStockId { get; set; }
        public string? ComponentType { get; set; }
        public string? BloodGroup { get; set; }
        public int AvailableQty { get; set; }
        public int ReservedQty { get; set; }
        public int QuarantinedQty { get; set; }
    }
}
