using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TomagochiApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public string InventoryId { get; set; }
    public string PetID { get; set; }
}