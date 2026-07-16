using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BloodCenterOS.API.IntegrationTests.Fixtures;

public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<ApiResult<T>> LoginAsync<T>(object request)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", request, JsonOptions);
        return await DeserializeAsync<T>(response);
    }

    protected async Task<ApiResult<T>> GetAsync<T>(string url, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (token != null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await Client.SendAsync(request);
        return await DeserializeAsync<T>(response);
    }

    protected async Task<ApiResult<T>> PostAsync<T>(string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (token != null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonOptions);
        var response = await Client.SendAsync(request);
        return await DeserializeAsync<T>(response);
    }

    protected async Task<ApiResult<T>> PutAsync<T>(string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        if (token != null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonOptions);
        var response = await Client.SendAsync(request);
        return await DeserializeAsync<T>(response);
    }

    protected async Task<string> GetTokenAsync()
    {
        var loginResult = await LoginAsync<LoginResponseData>(new { userName = "admin", password = "admin@123" });
        Assert.NotNull(loginResult.Data);
        return loginResult.Data.Token;
    }

    private static async Task<ApiResult<T>> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        try
        {
            var result = JsonSerializer.Deserialize<ApiResult<T>>(json, JsonOptions);
            return result ?? new ApiResult<T> { Success = false, Message = "Deserialization returned null" };
        }
        catch (JsonException ex)
        {
            return new ApiResult<T> { Success = false, Message = $"JSON parse error: {ex.Message}", StatusCode = (int)response.StatusCode };
        }
    }
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int StatusCode { get; set; }
}

public class LoginResponseData
{
    public string Token { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
