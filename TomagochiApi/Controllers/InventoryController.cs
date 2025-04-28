using Microsoft.AspNetCore.Mvc;
using TomagochiApi.Services;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventory(string id)
    {
        try
        {
            var inventory = await _inventoryService.GetInventory(id);
            return Ok(inventory);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
//добавить jwt token
    [HttpPost("{inventoryId}/add")]
    public async Task<IActionResult> AddItems(
        string inventoryId, 
        [FromQuery] string itemType,
        [FromQuery] int quantity)
    {
        try
        {
            var inventory = await _inventoryService.AddItemsToInventory(inventoryId, itemType, quantity);
            return Ok(inventory);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{inventoryId}/remove")]
    public async Task<IActionResult> RemoveItems(
        string inventoryId,
        [FromQuery] string itemType,
        [FromQuery] int quantity)
    {
        try
        {
            var inventory = await _inventoryService.RemoveItemsFromInventory(inventoryId, itemType, quantity);
            return Ok(inventory);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{inventoryId}/reset")]
    public async Task<IActionResult> ResetInventory(string inventoryId)
    {
        try
        {
            await _inventoryService.ResetInventory(inventoryId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}