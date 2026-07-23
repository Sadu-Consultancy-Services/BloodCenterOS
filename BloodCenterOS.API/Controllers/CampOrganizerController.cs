using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/camp-organizers")]
public class CampOrganizerController : ControllerBase
{
    private readonly ICampOrganizerRepository _repo;
    public CampOrganizerController(ICampOrganizerRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<CampOrganizer>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<CampOrganizer>.Fail("Organizer not found"));
        return Ok(ApiResponse<CampOrganizer>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CampOrganizer organizer)
    {
        organizer.CenterId = CenterId;
        var id = await _repo.CreateAsync(organizer);
        return Ok(ApiResponse<long>.Ok(id, "Organizer created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] CampOrganizer organizer)
    {
        organizer.OrganizerId = id;
        await _repo.UpdateAsync(organizer);
        return Ok(ApiResponse<object>.Ok(new { }, "Organizer updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Organizer deleted"));
    }
}
