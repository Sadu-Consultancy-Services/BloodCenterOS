namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class CollectionControllerTests : IntegrationTestBase
{
    public CollectionControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateCollection_WithAuth_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            campId = 1,
            donorId = 1,
            bloodBagNumber = "BAG-IT-001",
            bagBarcode = "8901234567890",
            bagLotNumber = "LOT-IT-001",
            bagVolumeMl = 450,
            collectionLocationType = "Camp",
            collectionStartTime = "2026-07-16T10:00:00Z",
            collectionEndTime = "2026-07-16T10:15:00Z",
            notes = "Integration test collection"
        };

        var result = await PostAsync<Collection>("/api/collections", body, token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.CollectionId > 0);
    }

    [Fact]
    public async Task GetCollection_ById_ReturnsCollection()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            campId = 1,
            donorId = 1,
            bloodBagNumber = "BAG-IT-002",
            bagBarcode = "8901234567891",
            bagLotNumber = "LOT-IT-002",
            bagVolumeMl = 350,
            collectionLocationType = "Camp",
            collectionStartTime = "2026-07-16T11:00:00Z",
            collectionEndTime = "2026-07-16T11:12:00Z",
            notes = "Integration test collection 2"
        };

        var created = await PostAsync<Collection>("/api/collections", body, token);
        Assert.True(created.Success);

        var result = await GetAsync<Collection>($"/api/collections/{created.Data.CollectionId}", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    public class Collection
    {
        public long CollectionId { get; set; }
        public long? CampId { get; set; }
        public long? DonorId { get; set; }
        public string? BloodBagNumber { get; set; }
        public string? Notes { get; set; }
    }
}
