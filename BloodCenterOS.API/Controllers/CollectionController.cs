using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/collections")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionRepository _collectionRepo;

    public CollectionController(ICollectionRepository collectionRepo)
    {
        _collectionRepo = collectionRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (!long.TryParse(centerId, out var cid))
                return BadRequest(ApiResponse<IEnumerable<Collection>>.Fail("Invalid center id"));

            var collections = await _collectionRepo.GetByCenterAsync(cid);
            return Ok(ApiResponse<IEnumerable<Collection>>.Ok(collections));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Collection>>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var collection = await _collectionRepo.GetByIdAsync(id);
            if (collection is null)
                return NotFound(ApiResponse<Collection>.Fail("Collection not found"));

            return Ok(ApiResponse<Collection>.Ok(collection));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Collection>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Collection collection)
    {
        try
        {
            var centerId = User.FindFirst("CenterId")?.Value;
            if (long.TryParse(centerId, out var cid))
                collection.CenterId = cid;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var uid))
                return BadRequest(ApiResponse<Collection>.Fail("Invalid user id"));

            var id = await _collectionRepo.CreateAsync(collection, uid);
            collection.CollectionId = id;
            return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Collection>.Ok(collection, "Collection created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Collection>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
