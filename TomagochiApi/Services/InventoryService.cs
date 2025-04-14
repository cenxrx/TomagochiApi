using MongoDB.Driver;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;

namespace TomagochiApi.Services;

public class InventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private static readonly HashSet<string> ValidItemTypes = new() { "Carrot", "Water", "Meat" };

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Inventory> GetInventory(string id)
    {
        var inventory = await _inventoryRepository.GetInventory(id);
        return inventory ?? throw new KeyNotFoundException("Inventory not found");
    }

    public async Task<Inventory> AddItemsToInventory(string inventoryId, string itemType, int quantity)
    {
        ValidateItemType(itemType);
        return await _inventoryRepository.AddItemToInventory(inventoryId, itemType, quantity);
    }

    public async Task<Inventory> RemoveItemsFromInventory(string inventoryId, string itemType, int quantity)
    {
        ValidateItemType(itemType);
        var result = await _inventoryRepository.RemoveItemfromInventory(inventoryId, itemType, quantity);
        return result ?? throw new InvalidOperationException("Not enough items to remove");
    }

    public async Task ResetInventory(string inventoryId)
    {
        var inventory = new Inventory()
        {
            id = inventoryId
        };
        await _inventoryRepository.UpdateInventory(inventoryId, inventory);
    }

    private void ValidateItemType(string itemType)
    {
        if (!ValidItemTypes.Contains(itemType))
            throw new ArgumentException($"Invalid item type: {itemType}");
    }

}