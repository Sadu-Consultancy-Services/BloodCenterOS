using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/sms-templates")]
public class SmsTemplateController : ControllerBase
{
    private readonly ISmsTemplateRepository _repo;
    public SmsTemplateController(ISmsTemplateRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<SmsTemplate>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<SmsTemplate>.Fail("Template not found"));
        return Ok(ApiResponse<SmsTemplate>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SmsTemplate template)
    {
        var id = await _repo.CreateAsync(CenterId, template.TemplateCode ?? "", template.TemplateText ?? "");
        return Ok(ApiResponse<long>.Ok(id, "SMS template created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] SmsTemplate template)
    {
        await _repo.UpdateAsync(id, template.TemplateCode, template.TemplateText);
        return Ok(ApiResponse<object>.Ok(new { }, "SMS template updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "SMS template deleted"));
    }
}
