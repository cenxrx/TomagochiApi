using TomagochiApi.Models;

namespace TomagochiApi.Interfaces;

public interface IPetRepository
{
    Task<Pet> CreatePet(Pet pet);
    Task<Pet> GetPet(string id);
    Task UpdatePet(string id, Pet pet);
    Task DeletePet(string id);
}