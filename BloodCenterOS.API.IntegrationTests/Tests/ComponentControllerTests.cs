namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class ComponentControllerTests : IntegrationTestBase
{
    public ComponentControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAvailableComponents_ReturnsList()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<List<Component>>("/api/components/available", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetAvailableComponents_FilteredByBloodGroup()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<List<Component>>("/api/components/available?bloodGroup=O%2B", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    public class Component
    {
        public long ComponentId { get; set; }
        public string? ComponentCode { get; set; }
        public string? ComponentType { get; set; }
        public string? CurrentStatus { get; set; }
    }
}
