using MongoDB.Driver;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;

namespace TomagochiApi.Services;

public class InventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    private static readonly HashSet<string> ValidItemTypes = new()
    {
        "Meet", "Fish", "Apple", "Carrot", "Soap", "Toilet_paper", "Toy", "Sleep"
    };

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Inventory> GetInventory(string id)
    {
        var inventory = await _inventoryRepository.GetInventory(id);
        return inventory ?? throw new KeyNotFoundException("Инвентарь не найден");
    }

    public async Task<Inventory> AddItemsToInventory(string inventoryId, string itemType, int quantity)
    {
        ValidateItemType(itemType);
        return await _inventoryRepository.AddItemToInventory(inventoryId, itemType, quantity);
    }

    public async Task<Inventory> RemoveItemsFromInventory(string inventoryId, string itemType, int quantity)
    {
        ValidateItemType(itemType);
        var result = await _inventoryRepository.RemoveItemFromInventory(inventoryId, itemType, quantity);
        return result ?? throw new InvalidOperationException("Недостаточно предметов для удаления");
    }

    public async Task ResetInventory(string inventoryId)
    {
        var inventory = new Inventory()
        {
            id = inventoryId
        };
        await _inventoryRepository.UpdateInventory(inventoryId, inventory);
    }

    public async Task<Inventory> CreateInventory()
    {
        var inventory = new Inventory();
        await _inventoryRepository.CreateInventory(inventory);
        return inventory;
    }

    private void ValidateItemType(string itemType)
    {
        if (!ValidItemTypes.Contains(itemType))
            throw new ArgumentException($"Недопустимый тип предмета: {itemType}");
    }
}