using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TomagochiApi.Models;

public class Inventory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public int CarrotCount { get; set; }
    public int WaterCount { get; set; }
    public int MeatCount { get; set; }
}