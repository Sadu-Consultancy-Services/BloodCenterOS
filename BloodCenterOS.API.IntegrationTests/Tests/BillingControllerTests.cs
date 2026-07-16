namespace BloodCenterOS.API.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class BillingControllerTests : IntegrationTestBase
{
    public BillingControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateBilling_WithAuth_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        var body = new
        {
            invoiceNumber = "INV-IT-001",
            patientId = 1,
            totalAmount = 1500.00,
            taxAmount = 150.00,
            discount = 0,
            paymentStatus = "Pending",
            paymentMode = "Cash"
        };

        var result = await PostAsync<Billing>("/api/billing", body, token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.BillingTransactionId > 0);
    }

    [Fact]
    public async Task AddPayment_WithAuth_ReturnsId()
    {
        var token = await GetTokenAsync();
        var createBody = new
        {
            invoiceNumber = "INV-IT-002",
            patientId = 1,
            totalAmount = 2000.00,
            taxAmount = 200.00,
            discount = 0,
            paymentStatus = "Pending",
            paymentMode = "Cash"
        };

        var created = await PostAsync<Billing>("/api/billing", createBody, token);
        Assert.True(created.Success);

        var url = $"/api/billing/{created.Data.BillingTransactionId}/payment?amount=2000&mode=Cash&reference=REF-IT-001";
        var result = await PostAsync<long>(url, null, token);

        Assert.True(result.Success);
        Assert.True(result.Data > 0);
    }

    public class Billing
    {
        public long BillingTransactionId { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
