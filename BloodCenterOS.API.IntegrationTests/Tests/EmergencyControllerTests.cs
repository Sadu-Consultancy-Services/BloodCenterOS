namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class EmergencyControllerTests : IntegrationTestBase
{
    public EmergencyControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateEmergencyRequest_WithAuth_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            hospitalId = 1,
            patientName = "Emergency Patient",
            bloodGroup = "O+",
            componentType = "PRBC",
            unitsRequired = 2,
            notes = "Integration test emergency"
        };

        var result = await PostAsync<EmergencyRequest>("/api/emergency/requests", body, token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.EmergencyRequestId > 0);
    }

    public class EmergencyRequest
    {
        public long EmergencyRequestId { get; set; }
        public string? PatientName { get; set; }
        public string? BloodGroup { get; set; }
        public string? ComponentType { get; set; }
    }
}
