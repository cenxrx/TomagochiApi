using MongoDB.Driver;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;

namespace TomagochiApi.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<Inventory> _inventoryCollection;

    public InventoryRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("tomagochidb");
        _inventoryCollection = database.GetCollection<Inventory>("Inventories");
    }

    public async Task<Inventory> CreateInventory(Inventory inventory)
    {
        await _inventoryCollection.InsertOneAsync(inventory);
        return inventory;
    }

    public async Task<Inventory> GetInventory(string id)
    {
        return await _inventoryCollection.Find(i => i.id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateInventory(string id, Inventory inventory)
    {
        await _inventoryCollection.ReplaceOneAsync(i => i.id == id, inventory);
    }

    public async Task DeleteInventory(string id)
    {
        await _inventoryCollection.DeleteOneAsync(i => i.id == id);
    }

    public async Task<Inventory> AddItemToInventory(string inventoryId, string itemType, int quantity)
    {
        var filter = Builders<Inventory>.Filter.Eq(i => i.id, inventoryId);
        var update = Builders<Inventory>.Update.Inc($"{itemType}Count", quantity);
        
        var options = new FindOneAndUpdateOptions<Inventory>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _inventoryCollection.FindOneAndUpdateAsync(filter, update, options);
    }

    public async Task<Inventory> RemoveItemfromInventory(string inventoryId, string itemType, int quantity)
    {
        var filter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(i => i.id, inventoryId),
            Builders<Inventory>.Filter.Gte($"{itemType}Count", quantity)
        );
        
        var update = Builders<Inventory>.Update.Inc($"{itemType}Count", -quantity);

        
        var options = new FindOneAndUpdateOptions<Inventory>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = false
        };

        try
        {
            return await _inventoryCollection.FindOneAndUpdateAsync(
                filter, 
                update, 
                options
            );
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}