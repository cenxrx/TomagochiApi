using MongoDB.Driver;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;

namespace TomagochiApi.Repositories;

public class PetRepository : IPetRepository
{
    private readonly IMongoCollection<Pet> _petCollection;

    public PetRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("tomagochidb");
        _petCollection = database.GetCollection<Pet>("Pets");
    }

    public async Task<Pet> CreatePet(Pet pet)
    {
        await _petCollection.InsertOneAsync(pet);
        return pet;
    }

    public async Task<Pet> GetPet(string id)
    {
        return await _petCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdatePet(string id, Pet pet)
    {
        await _petCollection.ReplaceOneAsync(p => p.Id == id, pet);
    }

    public async Task DeletePet(string id)
    {
        await _petCollection.DeleteOneAsync(p => p.Id == id);
    }
}