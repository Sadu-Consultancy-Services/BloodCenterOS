using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/replacement-donors")]
public class ReplacementDonorController : ControllerBase
{
    private readonly IReplacementDonorRepository _repo;
    public ReplacementDonorController(IReplacementDonorRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterReplacementDonorRequest request)
    {
        var id = await _repo.RegisterAsync(CenterId, request.RequestId, request.DonorId);
        return Ok(ApiResponse<long>.Ok(id, "Replacement donor registered"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<ReplacementDonor>>.Ok(data));
    }
}

public class RegisterReplacementDonorRequest
{
    public long RequestId { get; set; }
    public long DonorId { get; set; }
}
