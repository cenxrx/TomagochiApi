using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TomagochiApi.Models;

public class Pet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public int HungerCount { get; set; }
    public int HappinessCount { get; set; }
    public int HygieneCount { get; set; }
    public int EnergyCount { get; set; }
    public bool IsAlive { get; set; }
    public bool IsSleeping { get; set; } 

    public DateTime BirthDate { get; set; }          
    public DateTime LastFedTime { get; set; }         
    public DateTime LastCleanedTime { get; set; }     
    public DateTime LastSleepTime { get; set; }    
    public DateTime LastStateUpdateTime { get; set; } = DateTime.UtcNow;
}