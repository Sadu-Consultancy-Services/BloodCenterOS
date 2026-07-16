using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingRepository _repo;

    public SettingsController(ISettingRepository repo)
    {
        _repo = repo;
    }

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet("center-config")]
    public async Task<IActionResult> GetCenterConfig()
    {
        var data = await _repo.GetCenterConfigAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<CenterConfigItem>>.Ok(data));
    }

    [HttpPut("center-config")]
    public async Task<IActionResult> SetCenterConfig([FromBody] SetConfigRequest request)
    {
        await _repo.SetCenterConfigAsync(CenterId, request.Key, request.Value);
        return Ok(ApiResponse<object>.Ok(new { }, "Config saved"));
    }

    [HttpPut("center-config/batch")]
    public async Task<IActionResult> SetCenterConfigBatch([FromBody] List<SetConfigRequest> requests)
    {
        foreach (var r in requests)
            await _repo.SetCenterConfigAsync(CenterId, r.Key, r.Value);
        return Ok(ApiResponse<object>.Ok(new { }, "Configs saved"));
    }

    [HttpGet("system-config")]
    public async Task<IActionResult> GetSystemConfig()
    {
        var data = await _repo.GetSystemConfigAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<SystemConfigItem>>.Ok(data));
    }

    [HttpPut("system-config")]
    public async Task<IActionResult> SetSystemConfig([FromBody] SetConfigRequest request)
    {
        await _repo.SetSystemConfigAsync(CenterId, request.Key, request.Value, request.Description);
        return Ok(ApiResponse<object>.Ok(new { }, "Config saved"));
    }

    [HttpGet("lookup-types")]
    public async Task<IActionResult> GetLookupTypes()
    {
        var data = await _repo.GetLookupTypesAsync();
        return Ok(ApiResponse<IEnumerable<LookupTypeItem>>.Ok(data));
    }

    [HttpPost("lookup-types")]
    public async Task<IActionResult> CreateLookupType([FromBody] CreateLookupTypeRequest request)
    {
        var id = await _repo.CreateLookupTypeAsync(request.TypeCode, request.TypeName, request.Description);
        return Ok(ApiResponse<long>.Ok(id, "Lookup type created"));
    }

    [HttpGet("lookup-values/{typeId}")]
    public async Task<IActionResult> GetLookupValues(long typeId)
    {
        var data = await _repo.GetLookupValuesAsync(typeId, CenterId);
        return Ok(ApiResponse<IEnumerable<LookupValueItem>>.Ok(data));
    }

    [HttpPost("lookup-values")]
    public async Task<IActionResult> CreateLookupValue([FromBody] CreateLookupValueRequest request)
    {
        var id = await _repo.CreateLookupValueAsync(request.LookupTypeId, CenterId,
            request.ValueCode, request.ValueText, request.SortOrder, request.IsActive);
        return Ok(ApiResponse<long>.Ok(id, "Lookup value created"));
    }
}

public class SetConfigRequest
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Description { get; set; }
}

public class CreateLookupTypeRequest
{
    public string TypeCode { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
}

public class CreateLookupValueRequest
{
    public long LookupTypeId { get; set; }
    public string ValueCode { get; set; } = "";
    public string ValueText { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
