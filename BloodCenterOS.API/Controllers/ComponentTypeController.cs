using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/component-types")]
public class ComponentTypeController : ControllerBase
{
    private readonly IComponentTypeRepository _repo;
    public ComponentTypeController(IComponentTypeRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _repo.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ComponentType>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Not found"));
        return Ok(ApiResponse<ComponentType>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComponentTypeRequest request)
    {
        var id = await _repo.CreateAsync(request.ComponentTypeCode, request.Description);
        return Ok(ApiResponse<long>.Ok(id, "Component type created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateComponentTypeRequest request)
    {
        await _repo.UpdateAsync(id, request.ComponentTypeCode, request.Description);
        return Ok(ApiResponse<object>.Ok(new { }, "Component type updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { }, "Component type deleted"));
    }
}

public class CreateComponentTypeRequest
{
    public string ComponentTypeCode { get; set; } = "";
    public string? Description { get; set; }
}

public class UpdateComponentTypeRequest
{
    public string? ComponentTypeCode { get; set; }
    public string? Description { get; set; }
}
