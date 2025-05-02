using TomagochiApi.Models;

namespace TomagochiApi.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory> CreateInventory(Inventory inventory);
    Task<Inventory> GetInventory(string id);
    Task UpdateInventory(string id, Inventory inventory);
    Task DeleteInventory(string id);
    Task<Inventory> AddItemToInventory(string inventoryId, string itemType, int quantity);
    Task<Inventory> RemoveItemFromInventory(string inventoryId, string itemType, int quantity);
}