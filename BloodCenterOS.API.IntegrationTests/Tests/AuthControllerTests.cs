namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var result = await LoginAsync<LoginResponseData>(new { userName = "admin", password = "admin@123" });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Token);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var result = await LoginAsync<LoginResponseData>(new { userName = "admin", password = "wrong" });

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        var result = await LoginAsync<LoginResponseData>(new { userName = "nonexistent", password = "admin@123" });

        Assert.False(result.Success);
    }
}
