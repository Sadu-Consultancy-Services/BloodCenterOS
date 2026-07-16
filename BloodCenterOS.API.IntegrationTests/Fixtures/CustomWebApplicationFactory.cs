using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BloodCenterOS.API.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<BloodCenterOS.API.ApiStartup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
    }
}
