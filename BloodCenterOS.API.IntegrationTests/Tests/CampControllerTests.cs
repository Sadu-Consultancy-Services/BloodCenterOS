namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class CampControllerTests : IntegrationTestBase
{
    public CampControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateCamp_WithAuth_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            campCode = "IT-CAMP-" + Guid.NewGuid().ToString("N")[..8],
            campName = "Integration Test Camp",
            venue = "Test Venue",
            city = "Mumbai",
            campDate = "2026-12-25T00:00:00",
            totalDonorsExpected = 100
        };

        var result = await PostAsync<Camp>("/api/camps", body, token);

        Assert.True(result.Success, result.Message ?? "Unknown error");
        Assert.NotNull(result.Data);
        Assert.True(result.Data.CampId > 0);
    }

    [Fact]
    public async Task GetCamp_ById_ReturnsCamp()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            campCode = "IT-CAMP-GET-" + Guid.NewGuid().ToString("N")[..8],
            campName = "GetCamp Test",
            venue = "Test Venue",
            city = "Mumbai",
            campDate = "2026-12-26T00:00:00",
            totalDonorsExpected = 50
        };

        var created = await PostAsync<Camp>("/api/camps", body, token);
        Assert.True(created.Success, created.Message ?? "Create failed");

        var result = await GetAsync<Camp>($"/api/camps/{created.Data.CampId}", token);

        Assert.True(result.Success, result.Message ?? "Get failed");
        Assert.NotNull(result.Data);
        Assert.Equal("GetCamp Test", result.Data.CampName);
    }

    [Fact]
    public async Task GetUpcomingCamps_ReturnsCampList()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<List<Camp>>("/api/camps/upcoming", token);

        Assert.True(result.Success, result.Message ?? "Get upcoming failed");
        Assert.NotNull(result.Data);
    }

    public class Camp
    {
        public long CampId { get; set; }
        public string? CampName { get; set; }
        public string? City { get; set; }
        public string? Venue { get; set; }
        public string? CampCode { get; set; }
    }
}
