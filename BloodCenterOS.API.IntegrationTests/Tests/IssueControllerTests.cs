namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class IssueControllerTests : IntegrationTestBase
{
    public IssueControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetPendingRequests_ReturnsList()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<List<PatientRequest>>("/api/issues/ready-for-issue", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    public class PatientRequest
    {
        public long RequestId { get; set; }
        public string? PatientName { get; set; }
        public string? BloodGroup { get; set; }
        public string? ComponentType { get; set; }
    }
}
