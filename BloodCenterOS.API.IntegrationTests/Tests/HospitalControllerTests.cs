namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class HospitalControllerTests : IntegrationTestBase
{
    public HospitalControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateHospital_WithAuth_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            hospitalName = "Test Hospital",
            hospitalCode = "HOSP-IT-001",
            address = "123 Test Lane, Mumbai",
            contactPerson = "Dr. Test",
            phone = "022-12345678",
            email = "test@hospital.com"
        };

        var result = await PostAsync<Hospital>("/api/hospitals", body, token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.HospitalId > 0);
    }

    public class Hospital
    {
        public long HospitalId { get; set; }
        public string? HospitalName { get; set; }
    }
}
