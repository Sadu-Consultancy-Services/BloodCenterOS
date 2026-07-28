using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/store-inventory")]
public class StoreInventoryController : ControllerBase
{
    private readonly IStoreInventoryRepository _repo;
    public StoreInventoryController(IStoreInventoryRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet("items")]
    public async Task<IActionResult> GetItems() => Ok(ApiResponse<IEnumerable<InvItem>>.Ok(await _repo.GetItemsAsync(CenterId)));

    [HttpGet("items/active")]
    public async Task<IActionResult> GetActiveItems() => Ok(ApiResponse<IEnumerable<InvItem>>.Ok(await _repo.GetActiveItemsAsync(CenterId)));

    [HttpGet("items/{id}")]
    public async Task<IActionResult> GetItem(long id)
    {
        var item = await _repo.GetItemByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(ApiResponse<InvItem>.Ok(item));
    }

    [HttpPost("items")]
    public async Task<IActionResult> UpsertItem([FromBody] InvItem item)
    {
        var id = await _repo.UpsertItemAsync(CenterId, item, UserId);
        return Ok(ApiResponse<long>.Ok(id));
    }

    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteItem(long id)
    {
        await _repo.DeleteItemAsync(id);
        return Ok(ApiResponse<string>.Ok("Item deactivated"));
    }

    [HttpPost("inward")]
    public async Task<IActionResult> Inward([FromBody] InwardRequest req)
    {
        var id = await _repo.InwardAsync(CenterId, req.ItemId, req.Quantity, req.Description, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Stock received"));
    }

    [HttpPost("outward")]
    public async Task<IActionResult> Outward([FromBody] OutwardRequest req)
    {
        var id = await _repo.OutwardAsync(CenterId, req.ItemId, req.Quantity, req.Description, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Stock issued"));
    }

    [HttpGet("transactions/{itemId}")]
    public async Task<IActionResult> GetTransactions(long itemId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var items = await _repo.GetTransactionsAsync(CenterId, itemId, from, to);
        return Ok(ApiResponse<IEnumerable<InvTransaction>>.Ok(items));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() => Ok(ApiResponse<IEnumerable<InvStockSummary>>.Ok(await _repo.GetSummaryAsync(CenterId)));
}
