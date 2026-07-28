using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/storages")]
public class StorageController : ControllerBase
{
    private readonly IStorageRepository _repo;
    public StorageController(IStorageRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<StorageMaster>>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(ApiResponse<StorageMaster>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] StorageMaster item)
    {
        var id = await _repo.UpsertAsync(CenterId, item, UserId);
        return Ok(ApiResponse<long>.Ok(id));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Storage deactivated"));
    }
}
