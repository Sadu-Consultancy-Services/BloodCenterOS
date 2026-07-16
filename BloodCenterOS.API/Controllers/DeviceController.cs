using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/devices")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceRepository _repo;
    public DeviceController(IDeviceRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<Device>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<Device>.Fail("Device not found"));
        return Ok(ApiResponse<Device>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Device device)
    {
        device.CenterId = CenterId;
        var id = await _repo.CreateAsync(device);
        return Ok(ApiResponse<long>.Ok(id, "Device created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Device device)
    {
        device.DeviceId = id;
        await _repo.UpdateAsync(device);
        return Ok(ApiResponse<object>.Ok(new { }, "Device updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Device deleted"));
    }
}
