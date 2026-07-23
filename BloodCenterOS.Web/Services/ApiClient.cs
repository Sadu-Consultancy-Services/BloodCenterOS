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

    // ── Hospitals ──
    public Task<ApiResponse<List<Hospital>>?> GetHospitalsAsync() =>
        GetAsync<List<Hospital>>("/api/hospitals");

    public Task<ApiResponse<Hospital>?> CreateHospitalAsync(Hospital hospital) =>
        PostAsync<Hospital>("/api/hospitals", hospital);

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

    // ── Issues ──
    public Task<ApiResponse<List<PatientRequest>>?> GetPendingRequestsAsync() =>
        GetAsync<List<PatientRequest>>("/api/issues/pending-requests");

    public Task<ApiResponse<List<IssueRecord>>?> GetIssueHistoryAsync() =>
        GetAsync<List<IssueRecord>>("/api/issues");

    public Task<ApiResponse<IssueRecord>?> CreateIssueAsync(IssueRecord issue) =>
        PostAsync<IssueRecord>("/api/issues", issue);

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
}
