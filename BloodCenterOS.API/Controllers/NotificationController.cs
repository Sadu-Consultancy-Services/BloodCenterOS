using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _repo;
    public NotificationController(INotificationRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request)
    {
        var id = await _repo.CreateAsync(CenterId, request.Type, request.Title, request.Body, request.Audience);
        return Ok(ApiResponse<long>.Ok(id, "Notification created"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<Notification>>.Ok(data));
    }
}

public class CreateNotificationRequest
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Audience { get; set; } = "";
}
