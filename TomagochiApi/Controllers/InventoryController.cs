using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomagochiApi.Services;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;
    private readonly UserService _userService;

    public InventoryController(InventoryService inventoryService, UserService userService)
    {
        _inventoryService = inventoryService;
        _userService = userService;
    }

    /// <summary>
    /// Получить инвентарь по ID (без JWT)
    /// </summary>
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
            return NotFound("Инвентарь не найден");
        }
    }

    /// <summary>
    /// Создать новый инвентарь (требуется JWT)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateInventory()
    {
        try
        {
            var inventory = await _inventoryService.CreateInventory();
            return CreatedAtAction(nameof(GetInventory), new { id = inventory.id }, inventory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при создании инвентаря: {ex.Message}");
        }
    }

    /// <summary>
    /// Добавить предметы в инвентарь (требуется JWT)
    /// </summary>
    [HttpPost("/add")]
    [Authorize]
    public async Task<IActionResult> AddItems(
        [FromQuery] string itemName,
        [FromQuery] int quantity)
    {
        try
        {
            var inventoryId = User.FindFirst("InventoryId")?.Value;
            var inventory = await _inventoryService.AddItemsToInventory(inventoryId, itemName, quantity);
            return Ok(inventory);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message + "asdsa");
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Инвентарь не найден");
        }
    }

    /// <summary>
    /// Удалить предметы из инвентаря (требуется JWT)
    /// </summary>
    [HttpPost("/use")]
    [Authorize]
    public async Task<IActionResult> UseItems(
        [FromQuery] string itemName,
        [FromQuery] int quantity)
    {
        try
        {
            var inventoryId = User.FindFirst("InventoryId")?.Value;
            var inventory = await _inventoryService.RemoveItemsFromInventory(inventoryId, itemName, quantity);
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
            return NotFound("Инвентарь не найден");
        }
    }

    /// <summary>
    /// Сбросить инвентарь (требуется JWT)
    /// </summary>
    [HttpPost("/reset")]
    [Authorize]
    public async Task<IActionResult> ResetInventory()
    {
        try
        {
            var inventoryId = User.FindFirst("InventoryId")?.Value;
            await _inventoryService.ResetInventory(inventoryId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Инвентарь не найден");
        }
    }
}