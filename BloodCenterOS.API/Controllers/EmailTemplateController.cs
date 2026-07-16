using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/email-templates")]
public class EmailTemplateController : ControllerBase
{
    private readonly IEmailTemplateRepository _repo;
    public EmailTemplateController(IEmailTemplateRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<EmailTemplate>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<EmailTemplate>.Fail("Template not found"));
        return Ok(ApiResponse<EmailTemplate>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmailTemplate template)
    {
        var id = await _repo.CreateAsync(CenterId, template.TemplateCode ?? "", template.Subject ?? "", template.BodyHtml ?? "");
        return Ok(ApiResponse<long>.Ok(id, "Email template created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] EmailTemplate template)
    {
        await _repo.UpdateAsync(id, template.TemplateCode, template.Subject, template.BodyHtml);
        return Ok(ApiResponse<object>.Ok(new { }, "Email template updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Email template deleted"));
    }
}
