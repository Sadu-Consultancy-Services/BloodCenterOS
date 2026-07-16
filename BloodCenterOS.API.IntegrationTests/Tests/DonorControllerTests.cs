namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class DonorControllerTests : IntegrationTestBase
{
    private static readonly string UniqueSuffix = Guid.NewGuid().ToString("N")[..6];

    public DonorControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateDonor_WithAuth_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            firstName = "Create",
            lastName = "Donor_" + UniqueSuffix,
            gender = "Male",
            dateOfBirth = "1990-01-15T00:00:00",
            bloodGroup = "O+",
            phone = "998877" + Random.Shared.Next(1000, 9999),
            email = $"create.{UniqueSuffix}@example.com",
            city = "Mumbai",
            pincode = "400001",
            occupation = "Engineer"
        };

        var result = await PostAsync<Donor>("/api/donors", body, token);

        Assert.True(result.Success, result.Message ?? "Unknown error");
        Assert.NotNull(result.Data);
        Assert.True(result.Data.DonorId > 0);
    }

    [Fact]
    public async Task GetDonor_ById_ReturnsDonor()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            firstName = "Get",
            lastName = "DonorTest_" + UniqueSuffix,
            gender = "Female",
            dateOfBirth = "1985-06-20T00:00:00",
            bloodGroup = "A+",
            phone = "998877" + Random.Shared.Next(1000, 9999),
            email = $"get.{UniqueSuffix}@example.com",
            city = "Delhi"
        };

        var created = await PostAsync<Donor>("/api/donors", body, token);
        Assert.True(created.Success, created.Message ?? "Create failed");

        var result = await GetAsync<Donor>($"/api/donors/{created.Data.DonorId}", token);

        Assert.True(result.Success, result.Message ?? "Get failed");
        Assert.Equal("Get", result.Data.FirstName);
    }

    [Fact]
    public async Task UpdateDonor_UpdatesPhone()
    {
        var token = await GetTokenAsync();
        var phone = "998877" + Random.Shared.Next(1000, 9999);
        var body = new
        {
            firstName = "Update",
            lastName = "DonorTest_" + UniqueSuffix,
            gender = "Male",
            dateOfBirth = "1992-03-10T00:00:00",
            bloodGroup = "B+",
            phone = phone,
            email = $"update.{UniqueSuffix}@example.com",
            city = "Pune"
        };

        var created = await PostAsync<Donor>("/api/donors", body, token);
        Assert.True(created.Success, created.Message ?? "Create failed");

        var updateBody = new
        {
            firstName = "Update",
            lastName = "DonorTest_" + UniqueSuffix,
            gender = "Male",
            dateOfBirth = "1992-03-10T00:00:00",
            bloodGroup = "B+",
            phone = "9988770000",
            email = $"updated.{UniqueSuffix}@example.com",
            city = "Pune"
        };

        var result = await PutAsync<Donor>($"/api/donors/{created.Data.DonorId}", updateBody, token);

        Assert.True(result.Success, result.Message ?? "Update failed");
        Assert.Equal("9988770000", result.Data.Phone);
    }

    [Fact]
    public async Task SearchDonors_ReturnsPagedResults()
    {
        var token = await GetTokenAsync();

        var result = await GetAsync<PagedResult<Donor>>($"/api/donors/search?keyword=Test&page=1&size=10", token);

        Assert.True(result.Success, result.Message ?? "Search failed");
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Page == 1);
    }

    [Fact]
    public async Task GetDonor_WithoutAuth_ReturnsUnauthorized()
    {
        var result = await GetAsync<Donor>("/api/donors/1");

        Assert.False(result.Success);
    }

    public class Donor
    {
        public long DonorId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T>? Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
