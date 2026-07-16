using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterRepository _repo;
    public NewsletterController(INewsletterRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<NewsletterSubscription>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<NewsletterSubscription>.Fail("Subscription not found"));
        return Ok(ApiResponse<NewsletterSubscription>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNewsletterRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.Email);
        return Ok(ApiResponse<long>.Ok(id, "Subscription created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateNewsletterRequest request)
    {
        await _repo.UpdateAsync(id, request.Email, request.IsActive);
        return Ok(ApiResponse<object>.Ok(new { }, "Subscription updated"));
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(long id)
    {
        await _repo.ToggleActiveAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Status toggled"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Subscription deleted"));
    }
}

public class CreateNewsletterRequest
{
    public string Email { get; set; } = "";
}

public class UpdateNewsletterRequest
{
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}
