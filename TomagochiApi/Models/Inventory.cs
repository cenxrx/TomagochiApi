using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TomagochiApi.Models;

public class Item
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Count { get; set; }
}

public class Inventory
{
    public Inventory()
    {
        items = InitializeDefaultItems();
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }

    public List<Item> items { get; set; }

    private List<Item> InitializeDefaultItems()
    {
        return new List<Item>
        {
            new Item { Name = "Meet", Type = "food", Count = 1 },
            new Item { Name = "Fish", Type = "food", Count = 1 },
            new Item { Name = "Apple", Type = "food", Count = 1 },
            new Item { Name = "Carrot", Type = "food", Count = 1 },
            new Item { Name = "Soap", Type = "cleanItem", Count = 1 },
            new Item { Name = "Toilet_paper", Type = "cleanItem", Count = 1 },
            new Item { Name = "Toy", Type = "toy", Count = 1 },
            new Item { Name = "Sleep", Type = "sleepItem", Count = 1 }
        };
    }
}