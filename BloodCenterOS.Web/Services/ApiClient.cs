using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;

namespace BloodCenterOS.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiClient(HttpClient http, ITokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    private void SetAuthHeader()
    {
        var token = _tokenStore.Token;
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string path)
    {
        SetAuthHeader();
        var resp = await _http.GetAsync(path);
        var json = await resp.Content.ReadAsStringAsync();
        return Deserialize<T>(json);
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string path, object? body = null)
    {
        SetAuthHeader();
        var content = body != null ? ToJson(body) : null;
        var resp = await _http.PostAsync(path, content);
        var json = await resp.Content.ReadAsStringAsync();
        return Deserialize<T>(json);
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string path, object? body = null)
    {
        SetAuthHeader();
        var content = body != null ? ToJson(body) : null;
        var resp = await _http.PutAsync(path, content);
        var json = await resp.Content.ReadAsStringAsync();
        return Deserialize<T>(json);
    }

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string path)
    {
        SetAuthHeader();
        var resp = await _http.DeleteAsync(path);
        var json = await resp.Content.ReadAsStringAsync();
        return Deserialize<T>(json);
    }

    public async Task<ApiResponse<T>?> PostFormAsync<T>(string path, Dictionary<string, string?> form)
    {
        SetAuthHeader();
        var content = new FormUrlEncodedContent(form);
        var resp = await _http.PostAsync(path, content);
        var json = await resp.Content.ReadAsStringAsync();
        return Deserialize<T>(json);
    }

    private static ApiResponse<T>? Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOpts); }
        catch { return null; }
    }

    public async Task<byte[]?> GetByteArrayAsync(string path)
    {
        SetAuthHeader();
        var resp = await _http.GetAsync(path);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsByteArrayAsync();
    }

    public string BuildUrl(string path) => $"{_http.BaseAddress?.ToString().TrimEnd('/')}{path}";

    private static StringContent ToJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOpts);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    // ── Auth ──
    public Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest req) =>
        PostAsync<LoginResponse>("/api/auth/login", req);

    public Task<ApiResponse<object>?> LogoutAsync(long loginId) =>
        PostAsync<object>($"/api/login-history/{loginId}/logout");

    // ── Donors ──
    public Task<ApiResponse<Donor>?> CreateDonorAsync(Donor donor) =>
        PostAsync<Donor>("/api/donors", donor);

    public Task<ApiResponse<Donor>?> GetDonorAsync(long id) =>
        GetAsync<Donor>($"/api/donors/{id}");

    public Task<ApiResponse<Donor>?> UpdateDonorAsync(long id, Donor donor) =>
        PutAsync<Donor>($"/api/donors/{id}", donor);

    public Task<ApiResponse<PagedResult<Donor>>?> SearchDonorsAsync(string? keyword, string? bloodGroup, string? gender, int page = 1, int size = 20)
    {
        var q = new Dictionary<string, string?> { ["page"] = page.ToString(), ["size"] = size.ToString() };
        if (!string.IsNullOrEmpty(keyword)) q["keyword"] = keyword;
        if (!string.IsNullOrEmpty(bloodGroup)) q["bloodGroup"] = bloodGroup;
        if (!string.IsNullOrEmpty(gender)) q["gender"] = gender;
        var qs = string.Join("&", q.Where(kv => kv.Value != null).Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));
        return GetAsync<PagedResult<Donor>>($"/api/donors/search?{qs}");
    }

    public Task<ApiResponse<List<Donor>>?> GetDonorsByPhoneAsync(string phone) =>
        GetAsync<List<Donor>>($"/api/donors/by-phone?phone={Uri.EscapeDataString(phone)}");

    public Task<ApiResponse<List<Donation>>?> GetDonationsByDonorAsync(long donorId) =>
        GetAsync<List<Donation>>($"/api/donors/{donorId}/donations");

    // ── Camps ──
    public Task<ApiResponse<Camp>?> CreateCampAsync(Camp camp) =>
        PostAsync<Camp>("/api/camps", camp);

    public Task<ApiResponse<Camp>?> GetCampAsync(long id) =>
        GetAsync<Camp>($"/api/camps/{id}");

    public Task<ApiResponse<List<Camp>>?> GetUpcomingCampsAsync() =>
        GetAsync<List<Camp>>("/api/camps/upcoming");

    // ── Camp Organizers ──
    public Task<ApiResponse<List<CampOrganizer>>?> GetCampOrganizersAsync() =>
        GetAsync<List<CampOrganizer>>("/api/camp-organizers");
    public Task<ApiResponse<CampOrganizer>?> GetCampOrganizerAsync(long id) =>
        GetAsync<CampOrganizer>($"/api/camp-organizers/{id}");
    public Task<ApiResponse<long>?> CreateCampOrganizerAsync(object body) =>
        PostAsync<long>("/api/camp-organizers", body);
    public Task<ApiResponse<object>?> UpdateCampOrganizerAsync(long id, object body) =>
        PutAsync<object>($"/api/camp-organizers/{id}", body);
    public Task<ApiResponse<object>?> DeleteCampOrganizerAsync(long id) =>
        DeleteAsync<object>($"/api/camp-organizers/{id}");

    // ── Blood Reception (from MBB) ──
    public Task<ApiResponse<List<BloodReception>>?> GetBloodReceptionsAsync(DateTime? from = null, DateTime? to = null)
    {
        var q = new List<string>();
        if (from.HasValue) q.Add($"fromDate={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"toDate={to:yyyy-MM-dd}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<BloodReception>>($"/api/blood-reception{qs}");
    }
    public Task<ApiResponse<BloodReception>?> GetBloodReceptionAsync(long id) =>
        GetAsync<BloodReception>($"/api/blood-reception/{id}");
    public Task<ApiResponse<long>?> CreateBloodReceptionAsync(object body) =>
        PostAsync<long>("/api/blood-reception", body);

    // ── Procurement Register ──
    public Task<ApiResponse<List<ProcurementRegisterItem>>?> SearchProcurementRegisterAsync(
        string? bloodGroup, string? componentType, string? status, DateTime? from, DateTime? to, string? keyword)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(bloodGroup)) q.Add($"bloodGroup={Uri.EscapeDataString(bloodGroup)}");
        if (!string.IsNullOrEmpty(componentType)) q.Add($"componentType={Uri.EscapeDataString(componentType)}");
        if (!string.IsNullOrEmpty(status)) q.Add($"status={Uri.EscapeDataString(status)}");
        if (from.HasValue) q.Add($"fromDate={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"toDate={to:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(keyword)) q.Add($"keyword={Uri.EscapeDataString(keyword)}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<ProcurementRegisterItem>>($"/api/procurement/register{qs}");
    }
    public Task<ApiResponse<List<ProcurementRegisterSummaryRow>>?> GetProcurementSummaryAsync() =>
        GetAsync<List<ProcurementRegisterSummaryRow>>("/api/procurement/summary");

    // ── Rate Management ──
    public Task<ApiResponse<List<RateMaster>>?> GetRatesAsync() =>
        GetAsync<List<RateMaster>>("/api/rates");
    public Task<ApiResponse<RateMaster>?> GetRateAsync(long id) =>
        GetAsync<RateMaster>($"/api/rates/{id}");
    public Task<ApiResponse<long>?> UpsertRateAsync(object body) =>
        PostAsync<long>("/api/rates", body);
    public Task<ApiResponse<object>?> DeleteRateAsync(long id) =>
        DeleteAsync<object>($"/api/rates/{id}");

    // ── Patient Reservation ──
    public Task<ApiResponse<List<BloodRequest>>?> GetReservationsAsync(
        string? status = null, DateTime? from = null, DateTime? to = null, string? keyword = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(status)) q.Add($"status={Uri.EscapeDataString(status)}");
        if (from.HasValue) q.Add($"fromDate={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"toDate={to:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(keyword)) q.Add($"keyword={Uri.EscapeDataString(keyword)}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<BloodRequest>>($"/api/reservations{qs}");
    }
    public Task<ApiResponse<List<BloodRequest>>?> GetPendingReservationsAsync() =>
        GetAsync<List<BloodRequest>>("/api/reservations/pending");
    public Task<ApiResponse<BloodRequest>?> GetReservationAsync(long id) =>
        GetAsync<BloodRequest>($"/api/reservations/{id}");
    public Task<ApiResponse<long>?> CreateReservationAsync(object body) =>
        PostAsync<long>("/api/reservations", body);
    public Task<ApiResponse<object>?> CancelReservationAsync(long id, string? reason = null) =>
        PostAsync<object>($"/api/reservations/{id}/cancel", reason);
    public Task<ApiResponse<List<AvailableComponentItem>>?> GetAvailableComponentsAsync(
        string bloodGroup, string componentType, int units = 1)
    {
        var q = $"?bloodGroup={Uri.EscapeDataString(bloodGroup)}&componentType={Uri.EscapeDataString(componentType)}&units={units}";
        return GetAsync<List<AvailableComponentItem>>($"/api/reservations/available-components{q}");
    }

    // ── Hospitals ──
    public Task<ApiResponse<List<Hospital>>?> GetHospitalsAsync() =>
        GetAsync<List<Hospital>>("/api/hospitals");

    public Task<ApiResponse<Hospital>?> GetHospitalAsync(long id) =>
        GetAsync<Hospital>($"/api/hospitals/{id}");

    public Task<ApiResponse<Hospital>?> CreateHospitalAsync(Hospital hospital) =>
        PostAsync<Hospital>("/api/hospitals", hospital);

    public Task<ApiResponse<object>?> UpdateHospitalAsync(long id, object body) =>
        PutAsync<object>($"/api/hospitals/{id}", body);

    public Task<ApiResponse<object>?> DeleteHospitalAsync(long id) =>
        DeleteAsync<object>($"/api/hospitals/{id}");

    // ── Collections ──
    public Task<ApiResponse<List<Collection>>?> GetCollectionsAsync() =>
        GetAsync<List<Collection>>("/api/collections");

    public Task<ApiResponse<Collection>?> CreateCollectionAsync(Collection collection) =>
        PostAsync<Collection>("/api/collections", collection);

    public Task<ApiResponse<Collection>?> GetCollectionAsync(long id) =>
        GetAsync<Collection>($"/api/collections/{id}");

    // ── Components ──
    public Task<ApiResponse<List<Component>>?> GetAvailableComponentsAsync(string? bloodGroup = null)
    {
        var path = "/api/components/available";
        if (!string.IsNullOrEmpty(bloodGroup)) path += $"?bloodGroup={Uri.EscapeDataString(bloodGroup)}";
        return GetAsync<List<Component>>(path);
    }

    public Task<ApiResponse<long>?> PrepareComponentAsync(long bagId, string componentType, int volume) =>
        PostAsync<long>($"/api/components/prepare?bagId={bagId}&componentType={Uri.EscapeDataString(componentType)}&volume={volume}");

    // ── Inventory ──
    public Task<ApiResponse<List<InventoryStock>>?> GetStockAsync() =>
        GetAsync<List<InventoryStock>>("/api/inventory/stock");

    public Task<ApiResponse<object>?> GetSummaryAsync() =>
        GetAsync<object>("/api/inventory/summary");

    // ── Testing ──
    public Task<ApiResponse<long>?> CreateTestRecordAsync(BloodTestRecord record) =>
        PostAsync<long>("/api/tests", record);

    public Task<ApiResponse<List<BloodTestRecord>>?> GetPendingTestsAsync() =>
        GetAsync<List<BloodTestRecord>>("/api/tests/pending");

    public Task<ApiResponse<BloodTestRecord>?> GetTestRecordAsync(long id) =>
        GetAsync<BloodTestRecord>($"/api/tests/{id}");

    public Task<ApiResponse<List<BloodTestResult>>?> GetTestResultsAsync(long id) =>
        GetAsync<List<BloodTestResult>>($"/api/tests/{id}/results");

    public Task<ApiResponse<long>?> AddTestResultAsync(long id, BloodTestResult result) =>
        PostAsync<long>($"/api/tests/{id}/results", result);

    public Task<ApiResponse<object>?> CompleteTestRecordAsync(long id) =>
        PostAsync<object>($"/api/tests/{id}/complete");

    // ── Billing ──
    public Task<ApiResponse<List<Billing>>?> GetBillingsAsync() =>
        GetAsync<List<Billing>>("/api/billing");

    public Task<ApiResponse<Billing>?> CreateBillingAsync(Billing billing) =>
        PostAsync<Billing>("/api/billing", billing);

    public Task<ApiResponse<long>?> AddPaymentAsync(long billingId, decimal amount, string mode, string? reference = null)
    {
        var path = $"/api/billing/{billingId}/payment?amount={amount}&mode={Uri.EscapeDataString(mode)}";
        if (!string.IsNullOrEmpty(reference)) path += $"&reference={Uri.EscapeDataString(reference)}";
        return PostAsync<long>(path);
    }

    public Task<ApiResponse<InvoiceWithDetails>?> GetInvoiceAsync(long id) =>
        GetAsync<InvoiceWithDetails>($"/api/billing/{id}");

    public Task<ApiResponse<List<DuesRegisterItem>>?> GetDuesAsync(string? keyword = null)
    {
        var path = "/api/billing/dues";
        if (!string.IsNullOrEmpty(keyword)) path += $"?keyword={Uri.EscapeDataString(keyword)}";
        return GetAsync<List<DuesRegisterItem>>(path);
    }

    public Task<ApiResponse<long>?> CreateCreditNoteAsync(object body) =>
        PostAsync<long>("/api/billing/credit-note", body);

    // ── MBB Billing ──
    public Task<ApiResponse<List<MbbBill>>?> GetMbbBillsAsync() =>
        GetAsync<List<MbbBill>>("/api/mbb-bills");

    public Task<ApiResponse<MbbBillWithDetails>?> GetMbbBillAsync(long id) =>
        GetAsync<MbbBillWithDetails>($"/api/mbb-bills/{id}");

    public Task<ApiResponse<long>?> CreateMbbBillAsync(object body) =>
        PostAsync<long>("/api/mbb-bills", body);

    public Task<ApiResponse<object>?> PayMbbBillAsync(long id, decimal amount, string mode) =>
        PostAsync<object>($"/api/mbb-bills/{id}/payment?amount={amount}&mode={Uri.EscapeDataString(mode)}");

    // ── Discard ──
    public Task<ApiResponse<List<AvailableComponentForDiscard>>?> GetAvailableComponentsForDiscardAsync() =>
        GetAsync<List<AvailableComponentForDiscard>>("/api/discard/available-components");

    public Task<ApiResponse<List<DiscardRecord>>?> BulkDiscardAsync(object body) =>
        PostAsync<List<DiscardRecord>>("/api/discard/bulk", body);

    public Task<ApiResponse<List<DiscardRecord>>?> GetDiscardRegisterAsync(DateTime? from = null, DateTime? to = null, string? reason = null)
    {
        var q = new List<string>();
        if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(reason)) q.Add($"reason={Uri.EscapeDataString(reason)}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<DiscardRecord>>($"/api/discard{qs}");
    }

    public Task<ApiResponse<object>?> SetAutoclaveAsync(object body) =>
        PutAsync<object>("/api/discard/autoclave", body);

    public Task<ApiResponse<List<DiscardRecord>>?> GetAutoclaveRegisterAsync() =>
        GetAsync<List<DiscardRecord>>("/api/discard/autoclave-register");

    // ── Quality Control ──
    public Task<ApiResponse<List<QualityControl>>?> GetQcRecordsAsync(string? type = null, DateTime? from = null, DateTime? to = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(type)) q.Add($"type={Uri.EscapeDataString(type)}");
        if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<QualityControl>>($"/api/quality-control{qs}");
    }

    public Task<ApiResponse<QualityControl>?> GetQcRecordAsync(long id) =>
        GetAsync<QualityControl>($"/api/quality-control/{id}");

    public Task<ApiResponse<long>?> CreateQcRecordAsync(object body) =>
        PostAsync<long>("/api/quality-control", body);

    // ── Storage Master ──
    public Task<ApiResponse<List<StorageMaster>>?> GetStoragesAsync() =>
        GetAsync<List<StorageMaster>>("/api/storages");

    public Task<ApiResponse<StorageMaster>?> GetStorageAsync(long id) =>
        GetAsync<StorageMaster>($"/api/storages/{id}");

    public Task<ApiResponse<long>?> UpsertStorageAsync(object body) =>
        PostAsync<long>("/api/storages", body);

    public Task<ApiResponse<object>?> DeleteStorageAsync(long id) =>
        DeleteAsync<object>($"/api/storages/{id}");

    // ── Issue to Storage ──
    public Task<ApiResponse<List<AvailableComponentForStorage>>?> GetAvailableComponentsForStorageAsync() =>
        GetAsync<List<AvailableComponentForStorage>>("/api/issue-storage/available-components");

    public Task<ApiResponse<decimal>?> GetStorageRateAsync(long storageId, string componentType) =>
        GetAsync<decimal>($"/api/issue-storage/rate/{storageId}/{componentType}");

    public Task<ApiResponse<long>?> CreateIssueToStorageAsync(object body) =>
        PostAsync<long>("/api/issue-storage", body);

    public Task<ApiResponse<List<IssueStorageRecord>>?> GetIssueStorageRecordsAsync(long? storageId = null, DateTime? from = null, DateTime? to = null)
    {
        var q = new List<string>();
        if (storageId.HasValue) q.Add($"storageId={storageId}");
        if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<IssueStorageRecord>>($"/api/issue-storage{qs}");
    }

    public Task<ApiResponse<List<IssueStorageInvoice>>?> GetIssueStorageInvoicesAsync(long? storageId = null, DateTime? from = null, DateTime? to = null)
    {
        var q = new List<string>();
        if (storageId.HasValue) q.Add($"storageId={storageId}");
        if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<IssueStorageInvoice>>($"/api/issue-storage/invoices{qs}");
    }

    // ── Store Inventory ──
    public Task<ApiResponse<List<InvItem>>?> GetStoreItemsAsync() =>
        GetAsync<List<InvItem>>("/api/store-inventory/items");

    public Task<ApiResponse<List<InvItem>>?> GetActiveStoreItemsAsync() =>
        GetAsync<List<InvItem>>("/api/store-inventory/items/active");

    public Task<ApiResponse<InvItem>?> GetStoreItemAsync(long id) =>
        GetAsync<InvItem>($"/api/store-inventory/items/{id}");

    public Task<ApiResponse<long>?> UpsertStoreItemAsync(object body) =>
        PostAsync<long>("/api/store-inventory/items", body);

    public Task<ApiResponse<object>?> DeleteStoreItemAsync(long id) =>
        DeleteAsync<object>($"/api/store-inventory/items/{id}");

    public Task<ApiResponse<long>?> InwardStockAsync(object body) =>
        PostAsync<long>("/api/store-inventory/inward", body);

    public Task<ApiResponse<long>?> OutwardStockAsync(object body) =>
        PostAsync<long>("/api/store-inventory/outward", body);

    public Task<ApiResponse<List<InvTransaction>>?> GetStoreTransactionsAsync(long itemId, DateTime? from = null, DateTime? to = null)
    {
        var q = new List<string>();
        if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<InvTransaction>>($"/api/store-inventory/transactions/{itemId}{qs}");
    }

    public Task<ApiResponse<List<InvStockSummary>>?> GetStoreStockSummaryAsync() =>
        GetAsync<List<InvStockSummary>>("/api/store-inventory/summary");

    // ── Users ──
    public Task<ApiResponse<long>?> CreateUserAsync(object body) =>
        PostAsync<long>("/api/users", body);

    public Task<ApiResponse<object>?> UpdateUserAsync(long id, object body) =>
        PutAsync<object>($"/api/users/{id}", body);

    public Task<ApiResponse<object>?> ToggleUserLockAsync(long id, bool locked) =>
        PutAsync<object>($"/api/users/{id}/lock", new { locked });

    public Task<ApiResponse<object>?> AssignUserRoleAsync(long userId, long roleId) =>
        PostAsync<object>($"/api/users/{userId}/roles", new { roleId });

    public Task<ApiResponse<object>?> RemoveUserRoleAsync(long userId, long roleId) =>
        DeleteAsync<object>($"/api/users/{userId}/roles/{roleId}");

    // ── Roles ──
    public Task<ApiResponse<long>?> CreateRoleAsync(object body) =>
        PostAsync<long>("/api/roles", body);

    public Task<ApiResponse<object>?> AssignRolePermissionAsync(long roleId, long permissionId) =>
        PostAsync<object>($"/api/roles/{roleId}/permissions", new { permissionId });

    public Task<ApiResponse<object>?> RemoveRolePermissionAsync(long roleId, long permissionId) =>
        DeleteAsync<object>($"/api/roles/{roleId}/permissions/{permissionId}");

    // ── Reports ──
    public Task<ApiResponse<List<DonorSummaryRow>>?> GetDonorSummaryAsync(DateTime from, DateTime to) =>
        GetAsync<List<DonorSummaryRow>>($"/api/reports/donor-summary?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    public Task<ApiResponse<List<InventorySummaryRow>>?> GetInventorySummaryAsync() =>
        GetAsync<List<InventorySummaryRow>>("/api/reports/inventory-summary");

    public Task<ApiResponse<List<CampSummaryRow>>?> GetCampSummaryAsync(DateTime from, DateTime to) =>
        GetAsync<List<CampSummaryRow>>($"/api/reports/camp-summary?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    // ── Phase 9 Reports ──
    public Task<ApiResponse<List<BloodStockRow>>?> GetBloodStockReportAsync() =>
        GetAsync<List<BloodStockRow>>("/api/reports/blood-stock");

    public Task<ApiResponse<List<ProcurementSummaryRow>>?> GetProcurementSummaryAsync(DateTime from, DateTime to) =>
        GetAsync<List<ProcurementSummaryRow>>($"/api/reports/procurement-summary?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    public Task<ApiResponse<List<DonorListRow>>?> GetDonorListReportAsync(DateTime from, DateTime to, bool showContact = true) =>
        GetAsync<List<DonorListRow>>($"/api/reports/donor-list?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}&showContact={showContact}");

    public Task<ApiResponse<List<CmIncomeRow>>?> GetCmIncomeReportAsync(DateTime from, DateTime to) =>
        GetAsync<List<CmIncomeRow>>($"/api/reports/cm-income?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    public Task<ApiResponse<List<DiscountDetailRow>>?> GetDiscountDetailsReportAsync(DateTime from, DateTime to) =>
        GetAsync<List<DiscountDetailRow>>($"/api/reports/discount-details?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    public Task<ApiResponse<List<DailyIssueRow>>?> GetDailyIssuesReportAsync(DateTime from, DateTime to) =>
        GetAsync<List<DailyIssueRow>>($"/api/reports/daily-issues?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    public Task<ApiResponse<List<MbbInwardRow>>?> GetMbbInwardReportAsync(DateTime from, DateTime to, string? supplier = null)
    {
        var q = $"/api/reports/mbb-inward?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";
        if (!string.IsNullOrEmpty(supplier)) q += $"&supplier={Uri.EscapeDataString(supplier)}";
        return GetAsync<List<MbbInwardRow>>(q);
    }

    public Task<ApiResponse<List<QcDailyRow>>?> GetQcDailyReportAsync(DateTime date) =>
        GetAsync<List<QcDailyRow>>($"/api/reports/qc-daily?date={date:yyyy-MM-dd}");

    public Task<ApiResponse<List<InvStockRow>>?> GetInvStockReportAsync() =>
        GetAsync<List<InvStockRow>>("/api/reports/inv-stock");

    public Task<ApiResponse<List<InvInOutRow>>?> GetInvInOutReportAsync(DateTime from, DateTime to, string? type = null, string? itemIds = null)
    {
        var q = $"/api/reports/inv-inout?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";
        if (!string.IsNullOrEmpty(type)) q += $"&type={type}";
        if (!string.IsNullOrEmpty(itemIds)) q += $"&itemIds={itemIds}";
        return GetAsync<List<InvInOutRow>>(q);
    }

    public Task<ApiResponse<List<InvoiceDetailRow>>?> GetInvoiceDetailReportAsync(long invoiceId) =>
        GetAsync<List<InvoiceDetailRow>>($"/api/reports/invoice-detail/{invoiceId}");

    public Task<ApiResponse<List<BsInvoiceDetailRow>>?> GetBsInvoiceDetailReportAsync(long invoiceId) =>
        GetAsync<List<BsInvoiceDetailRow>>($"/api/reports/bs-invoice-detail/{invoiceId}");

    public Task<ApiResponse<List<CrossMatchReportRow>>?> GetCrossMatchReportAsync(long invoiceId) =>
        GetAsync<List<CrossMatchReportRow>>($"/api/reports/crossmatch-report/{invoiceId}");

    public Task<ApiResponse<List<DiscardRegisterRow>>?> GetDiscardRegisterReportAsync(DateTime from, DateTime to, string? reason = null)
    {
        var q = $"/api/reports/discard-register?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";
        if (!string.IsNullOrEmpty(reason)) q += $"&reason={Uri.EscapeDataString(reason)}";
        return GetAsync<List<DiscardRegisterRow>>(q);
    }

    public Task<ApiResponse<List<DuesRegisterRow>>?> GetDuesRegisterReportAsync(DateTime? asOnDate = null)
    {
        var q = "/api/reports/dues-register";
        if (asOnDate.HasValue) q += $"?asOnDate={asOnDate:yyyy-MM-dd}";
        return GetAsync<List<DuesRegisterRow>>(q);
    }

    public Task<ApiResponse<List<DiscardRegisterRow>>?> GetAutoclaveRegisterReportAsync(DateTime from, DateTime to) =>
        GetAsync<List<DiscardRegisterRow>>($"/api/reports/autoclave-register?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

    // ── Settings ──
    public Task<ApiResponse<List<CenterConfigItem>>?> GetCenterConfigAsync() =>
        GetAsync<List<CenterConfigItem>>("/api/settings/center-config");

    public Task<ApiResponse<object>?> SaveCenterConfigsAsync(List<SetConfigRequest> configs) =>
        PutAsync<object>("/api/settings/center-config/batch", configs);

    public Task<ApiResponse<List<SystemConfigItem>>?> GetSystemConfigAsync() =>
        GetAsync<List<SystemConfigItem>>("/api/settings/system-config");

    public Task<ApiResponse<object>?> SaveSystemConfigAsync(string key, string value) =>
        PutAsync<object>("/api/settings/system-config", new { key, value });

    public Task<ApiResponse<List<LookupTypeItem>>?> GetLookupTypesAsync() =>
        GetAsync<List<LookupTypeItem>>("/api/settings/lookup-types");

    public Task<ApiResponse<List<LookupValueItem>>?> GetLookupValuesAsync(long typeId) =>
        GetAsync<List<LookupValueItem>>($"/api/settings/lookup-values/{typeId}");

    public Task<ApiResponse<long>?> CreateLookupTypeAsync(object body) =>
        PostAsync<long>("/api/settings/lookup-types", body);

    public Task<ApiResponse<long>?> CreateLookupValueAsync(object body) =>
        PostAsync<long>("/api/settings/lookup-values", body);

    // ── Branches ──
    public Task<ApiResponse<List<Branch>>?> GetBranchesAsync() =>
        GetAsync<List<Branch>>("/api/branches");

    public Task<ApiResponse<Branch>?> GetBranchAsync(long id) =>
        GetAsync<Branch>($"/api/branches/{id}");

    public Task<ApiResponse<long>?> CreateBranchAsync(object body) =>
        PostAsync<long>("/api/branches", body);

    public Task<ApiResponse<object>?> UpdateBranchAsync(long id, object body) =>
        PutAsync<object>($"/api/branches/{id}", body);

    public Task<ApiResponse<object>?> DeleteBranchAsync(long id) =>
        DeleteAsync<object>($"/api/branches/{id}");

    // ── Departments ──
    public Task<ApiResponse<List<Department>>?> GetDepartmentsAsync() =>
        GetAsync<List<Department>>("/api/departments");

    public Task<ApiResponse<Department>?> GetDepartmentAsync(long id) =>
        GetAsync<Department>($"/api/departments/{id}");

    public Task<ApiResponse<long>?> CreateDepartmentAsync(object body) =>
        PostAsync<long>("/api/departments", body);

    public Task<ApiResponse<object>?> UpdateDepartmentAsync(long id, object body) =>
        PutAsync<object>($"/api/departments/{id}", body);

    public Task<ApiResponse<object>?> DeleteDepartmentAsync(long id) =>
        DeleteAsync<object>($"/api/departments/{id}");

    // ── Designations ──
    public Task<ApiResponse<List<Designation>>?> GetDesignationsAsync() =>
        GetAsync<List<Designation>>("/api/designations");

    public Task<ApiResponse<Designation>?> GetDesignationAsync(long id) =>
        GetAsync<Designation>($"/api/designations/{id}");

    public Task<ApiResponse<long>?> CreateDesignationAsync(object body) =>
        PostAsync<long>("/api/designations", body);

    public Task<ApiResponse<object>?> UpdateDesignationAsync(long id, object body) =>
        PutAsync<object>($"/api/designations/{id}", body);

    public Task<ApiResponse<object>?> DeleteDesignationAsync(long id) =>
        DeleteAsync<object>($"/api/designations/{id}");

    // ── Employees ──
    public Task<ApiResponse<List<Employee>>?> GetEmployeesAsync() =>
        GetAsync<List<Employee>>("/api/employees");

    public Task<ApiResponse<Employee>?> GetEmployeeAsync(long id) =>
        GetAsync<Employee>($"/api/employees/{id}");

    public Task<ApiResponse<long>?> CreateEmployeeAsync(object body) =>
        PostAsync<long>("/api/employees", body);

    public Task<ApiResponse<object>?> UpdateEmployeeAsync(long id, object body) =>
        PutAsync<object>($"/api/employees/{id}", body);

    public Task<ApiResponse<object>?> ToggleEmployeeActiveAsync(long id) =>
        PutAsync<object>($"/api/employees/{id}/toggle-active");

    public Task<ApiResponse<object>?> DeleteEmployeeAsync(long id) =>
        DeleteAsync<object>($"/api/employees/{id}");

    // ── Devices ──
    public Task<ApiResponse<List<Device>>?> GetDevicesAsync() =>
        GetAsync<List<Device>>("/api/devices");

    public Task<ApiResponse<Device>?> GetDeviceAsync(long id) =>
        GetAsync<Device>($"/api/devices/{id}");

    public Task<ApiResponse<long>?> CreateDeviceAsync(object body) =>
        PostAsync<long>("/api/devices", body);

    public Task<ApiResponse<object>?> UpdateDeviceAsync(long id, object body) =>
        PutAsync<object>($"/api/devices/{id}", body);

    public Task<ApiResponse<object>?> DeleteDeviceAsync(long id) =>
        DeleteAsync<object>($"/api/devices/{id}");

    // ── Fridges ──
    public Task<ApiResponse<List<Fridge>>?> GetFridgesAsync() =>
        GetAsync<List<Fridge>>("/api/fridges");

    public Task<ApiResponse<Fridge>?> GetFridgeAsync(long id) =>
        GetAsync<Fridge>($"/api/fridges/{id}");

    public Task<ApiResponse<long>?> CreateFridgeAsync(object body) =>
        PostAsync<long>("/api/fridges", body);

    public Task<ApiResponse<object>?> UpdateFridgeAsync(long id, object body) =>
        PutAsync<object>($"/api/fridges/{id}", body);

    public Task<ApiResponse<object>?> DeleteFridgeAsync(long id) =>
        DeleteAsync<object>($"/api/fridges/{id}");

    // ── Newsletter Subscriptions ──
    public Task<ApiResponse<List<NewsletterSubscription>>?> GetNewsletterSubscriptionsAsync() =>
        GetAsync<List<NewsletterSubscription>>("/api/newsletter");

    public Task<ApiResponse<NewsletterSubscription>?> GetNewsletterSubscriptionAsync(long id) =>
        GetAsync<NewsletterSubscription>($"/api/newsletter/{id}");

    public Task<ApiResponse<long>?> CreateNewsletterSubscriptionAsync(object body) =>
        PostAsync<long>("/api/newsletter", body);

    public Task<ApiResponse<object>?> UpdateNewsletterSubscriptionAsync(long id, object body) =>
        PutAsync<object>($"/api/newsletter/{id}", body);

    public Task<ApiResponse<object>?> ToggleNewsletterActiveAsync(long id) =>
        PutAsync<object>($"/api/newsletter/{id}/toggle-active");

    public Task<ApiResponse<object>?> DeleteNewsletterSubscriptionAsync(long id) =>
        DeleteAsync<object>($"/api/newsletter/{id}");

    // ── SMS Templates ──
    public Task<ApiResponse<List<SmsTemplate>>?> GetSmsTemplatesAsync() =>
        GetAsync<List<SmsTemplate>>("/api/sms-templates");

    public Task<ApiResponse<SmsTemplate>?> GetSmsTemplateAsync(long id) =>
        GetAsync<SmsTemplate>($"/api/sms-templates/{id}");

    public Task<ApiResponse<long>?> CreateSmsTemplateAsync(object body) =>
        PostAsync<long>("/api/sms-templates", body);

    public Task<ApiResponse<object>?> UpdateSmsTemplateAsync(long id, object body) =>
        PutAsync<object>($"/api/sms-templates/{id}", body);

    public Task<ApiResponse<object>?> DeleteSmsTemplateAsync(long id) =>
        DeleteAsync<object>($"/api/sms-templates/{id}");

    // ── Email Templates ──
    public Task<ApiResponse<List<EmailTemplate>>?> GetEmailTemplatesAsync() =>
        GetAsync<List<EmailTemplate>>("/api/email-templates");

    public Task<ApiResponse<EmailTemplate>?> GetEmailTemplateAsync(long id) =>
        GetAsync<EmailTemplate>($"/api/email-templates/{id}");

    public Task<ApiResponse<long>?> CreateEmailTemplateAsync(object body) =>
        PostAsync<long>("/api/email-templates", body);

    public Task<ApiResponse<object>?> UpdateEmailTemplateAsync(long id, object body) =>
        PutAsync<object>($"/api/email-templates/{id}", body);

    public Task<ApiResponse<object>?> DeleteEmailTemplateAsync(long id) =>
        DeleteAsync<object>($"/api/email-templates/{id}");

    // ── Emergency ──
    public Task<ApiResponse<List<EmergencyRequest>>?> GetPendingEmergencyRequestsAsync() =>
        GetAsync<List<EmergencyRequest>>("/api/emergency/requests/pending");

    public Task<ApiResponse<EmergencyRequest>?> CreateEmergencyRequestAsync(EmergencyRequest request) =>
        PostAsync<EmergencyRequest>("/api/emergency/requests", request);

    // ── Camp Inventory ──
    public Task<ApiResponse<List<CampInventory>>?> GetCampInventoryAsync(long? campId = null) =>
        GetAsync<List<CampInventory>>($"/api/camp-inventory{(campId.HasValue ? $"?campId={campId}" : "")}");

    public Task<ApiResponse<long>?> CreateCampInventoryAsync(object body) =>
        PostAsync<long>("/api/camp-inventory", body);

    public Task<ApiResponse<object>?> UpdateCampInventoryAsync(long id, object body) =>
        PutAsync<object>($"/api/camp-inventory/{id}", body);

    public Task<ApiResponse<object>?> DeleteCampInventoryAsync(long id) =>
        DeleteAsync<object>($"/api/camp-inventory/{id}");

    // ── Camp Expenses ──
    public Task<ApiResponse<List<CampExpense>>?> GetCampExpensesAsync(long? campId = null) =>
        GetAsync<List<CampExpense>>($"/api/camp-expenses{(campId.HasValue ? $"?campId={campId}" : "")}");

    public Task<ApiResponse<long>?> CreateCampExpenseAsync(object body) =>
        PostAsync<long>("/api/camp-expenses", body);

    public Task<ApiResponse<object>?> UpdateCampExpenseAsync(long id, object body) =>
        PutAsync<object>($"/api/camp-expenses/{id}", body);

    public Task<ApiResponse<object>?> DeleteCampExpenseAsync(long id) =>
        DeleteAsync<object>($"/api/camp-expenses/{id}");

    // ── Component Types ──
    public Task<ApiResponse<List<ComponentType>>?> GetComponentTypesAsync() =>
        GetAsync<List<ComponentType>>("/api/component-types");

    public Task<ApiResponse<long>?> CreateComponentTypeAsync(object body) =>
        PostAsync<long>("/api/component-types", body);

    public Task<ApiResponse<object>?> UpdateComponentTypeAsync(long id, object body) =>
        PutAsync<object>($"/api/component-types/{id}", body);

    public Task<ApiResponse<object>?> DeleteComponentTypeAsync(long id) =>
        DeleteAsync<object>($"/api/component-types/{id}");

    // ── Audit Logs ──
    public Task<ApiResponse<List<AuditLog>>?> GetAuditLogsAsync(long? userId = null, string? tableName = null, int limit = 100)
    {
        var q = $"/api/audit-logs?limit={limit}";
        if (userId.HasValue) q += $"&userId={userId}";
        if (!string.IsNullOrEmpty(tableName)) q += $"&tableName={Uri.EscapeDataString(tableName)}";
        return GetAsync<List<AuditLog>>(q);
    }

    // ── Login History ──
    public Task<ApiResponse<List<LoginHistory>>?> GetLoginHistoryAsync(long? userId = null, DateTime? fromDate = null, DateTime? toDate = null, int limit = 200)
    {
        var q = $"/api/login-history?limit={limit}";
        if (userId.HasValue) q += $"&userId={userId}";
        if (fromDate.HasValue) q += $"&fromDate={fromDate:yyyy-MM-ddTHH:mm:ss}";
        if (toDate.HasValue) q += $"&toDate={toDate:yyyy-MM-ddTHH:mm:ss}";
        return GetAsync<List<LoginHistory>>(q);
    }

    // ── Cross Matching ──
    public Task<ApiResponse<List<CrossMatchEntry>>?> GetCrossMatchPendingReservationsAsync() =>
        GetAsync<List<CrossMatchEntry>>("/api/crossmatches/pending-reservations");

    public Task<ApiResponse<List<CrossMatchEntry>>?> GetCrossMatchesAsync(string? status = null, DateTime? from = null, DateTime? to = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(status)) q.Add($"status={Uri.EscapeDataString(status)}");
        if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<List<CrossMatchEntry>>($"/api/crossmatches{qs}");
    }

    public Task<ApiResponse<CrossMatchWithTests>?> GetCrossMatchAsync(long id) =>
        GetAsync<CrossMatchWithTests>($"/api/crossmatches/{id}");

    public Task<ApiResponse<long>?> StartCrossMatchAsync(object body) =>
        PostAsync<long>("/api/crossmatches/start", body);

    public Task<ApiResponse<object>?> SetCrossMatchResultAsync(object body) =>
        PutAsync<object>("/api/crossmatches/set-result", body);

    public Task<ApiResponse<object>?> RejectCrossMatchComponentAsync(long testResultId) =>
        PostAsync<object>($"/api/crossmatches/reject-component/{testResultId}");

    // ── Blood Issuing ──
    public Task<ApiResponse<List<IssueRecord>>?> GetIssueHistoryAsync() =>
        GetAsync<List<IssueRecord>>("/api/issues");

    public Task<ApiResponse<List<ReservationReadyForIssue>>?> GetReadyForIssueAsync() =>
        GetAsync<List<ReservationReadyForIssue>>("/api/issues/ready-for-issue");

    public Task<ApiResponse<List<IssueRecord>>?> GetIssuesByReservationAsync(long reservationId) =>
        GetAsync<List<IssueRecord>>($"/api/issues/by-reservation/{reservationId}");

    public Task<ApiResponse<long>?> IssueFromReservationAsync(object body) =>
        PostAsync<long>("/api/issues/from-reservation", body);

    // ── Phase 10: Housekeeping Features ──

    // Patient Requests
    public Task<ApiResponse<List<PatientRequest>>?> GetPatientRequestsAsync() =>
        GetAsync<List<PatientRequest>>("/api/patient-requests");

    public Task<ApiResponse<List<PatientRequest>>?> GetPendingPatientRequestsAsync() =>
        GetAsync<List<PatientRequest>>("/api/patient-requests/pending");

    public Task<ApiResponse<PatientRequest>?> GetPatientRequestAsync(long id) =>
        GetAsync<PatientRequest>($"/api/patient-requests/{id}");

    public Task<ApiResponse<long>?> CreatePatientRequestAsync(object body) =>
        PostAsync<long>("/api/patient-requests", body);

    // Expense
    public Task<ApiResponse<List<Expense>>?> GetExpensesAsync(DateTime? from = null, DateTime? to = null)
    {
        var q = "/api/expenses";
        var p = new List<string>();
        if (from.HasValue) p.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) p.Add($"to={to:yyyy-MM-dd}");
        if (p.Count > 0) q += "?" + string.Join("&", p);
        return GetAsync<List<Expense>>(q);
    }

    public Task<ApiResponse<long>?> CreateExpenseAsync(object body) =>
        PostAsync<long>("/api/expenses", body);

    // Donor Appointments
    public Task<ApiResponse<List<DonorAppointment>>?> GetAppointmentsAsync(long? donorId = null)
    {
        var q = "/api/appointments";
        if (donorId.HasValue) q += $"?donorId={donorId}";
        return GetAsync<List<DonorAppointment>>(q);
    }

    public Task<ApiResponse<long>?> CreateAppointmentAsync(object body) =>
        PostAsync<long>("/api/appointments", body);

    public Task<ApiResponse<object>?> UpdateAppointmentStatusAsync(long id, string status) =>
        PutAsync<object>($"/api/appointments/{id}/status", new { status });

    // Blood Returns
    public Task<ApiResponse<List<ReturnRecord>>?> GetReturnsAsync() =>
        GetAsync<List<ReturnRecord>>("/api/returns");

    public Task<ApiResponse<long>?> CreateReturnAsync(object body) =>
        PostAsync<long>("/api/returns", body);

    // Deferrals
    public Task<ApiResponse<List<DeferralRecord>>?> GetActiveDeferralsAsync(long donorId) =>
        GetAsync<List<DeferralRecord>>($"/api/deferrals/active/{donorId}");

    public Task<ApiResponse<long>?> CreateDeferralAsync(object body) =>
        PostAsync<long>("/api/deferrals", body);

    // Donor Health
    public Task<ApiResponse<List<DonorHealth>>?> GetDonorHealthAsync(long donorId) =>
        GetAsync<List<DonorHealth>>($"/api/donors/{donorId}/health");

    public Task<ApiResponse<long>?> CreateDonorHealthAsync(long donorId, object body) =>
        PostAsync<long>($"/api/donors/{donorId}/health", body);

    // Test Kits
    public Task<ApiResponse<List<TestKit>>?> GetTestKitsAsync() =>
        GetAsync<List<TestKit>>("/api/test-kits");

    public Task<ApiResponse<long>?> CreateTestKitAsync(object body) =>
        PostAsync<long>("/api/test-kits", body);

    // Notifications
    public Task<ApiResponse<List<Notification>>?> GetNotificationsAsync() =>
        GetAsync<List<Notification>>("/api/notifications");

    public Task<ApiResponse<long>?> CreateNotificationAsync(object body) =>
        PostAsync<long>("/api/notifications", body);

    // Replacement Donors
    public Task<ApiResponse<List<ReplacementDonor>>?> GetReplacementDonorsAsync() =>
        GetAsync<List<ReplacementDonor>>("/api/replacement-donors");

    public Task<ApiResponse<long>?> RegisterReplacementDonorAsync(object body) =>
        PostAsync<long>("/api/replacement-donors", body);

    // Blood Bags
    public Task<ApiResponse<List<BloodBag>>?> SearchBloodBagsAsync(string? term = null)
    {
        var q = "/api/blood-bags";
        if (!string.IsNullOrEmpty(term)) q += $"?term={Uri.EscapeDataString(term)}";
        return GetAsync<List<BloodBag>>(q);
    }

    public Task<ApiResponse<BloodBag>?> GetBloodBagByNumberAsync(string bagNo) =>
        GetAsync<BloodBag>($"/api/blood-bags/{Uri.EscapeDataString(bagNo)}");

    public Task<ApiResponse<object>?> UpdateBloodBagStatusAsync(long bagId, string status) =>
        PutAsync<object>($"/api/blood-bags/{bagId}/status", new { status });

    // Component Log (store/transfer/discard)
    public Task<ApiResponse<long>?> StoreComponentAsync(long componentId, object body) =>
        PostAsync<long>($"/api/components/{componentId}/store", body);

    public Task<ApiResponse<long>?> TransferComponentAsync(long componentId, object body) =>
        PostAsync<long>($"/api/components/{componentId}/transfer", body);

    public Task<ApiResponse<long>?> DiscardComponentAsync(long componentId, object body) =>
        PostAsync<long>($"/api/components/{componentId}/discard", body);

    public Task<ApiResponse<object>?> UpdateComponentStatusAsync(long componentId, string status) =>
        PutAsync<object>($"/api/components/{componentId}/status", new { status });
}
