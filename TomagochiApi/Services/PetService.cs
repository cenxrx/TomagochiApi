using TomagochiApi.Interfaces;
using TomagochiApi.Models;
using System;

namespace TomagochiApi.Services;

public class PetService
{
    private readonly IPetRepository _petRepository;
    private readonly IUserRepository _userRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public PetService(
        IPetRepository petRepository,
        IUserRepository userRepository,
        IInventoryRepository inventoryRepository,
        UserService userService)
    {
        _petRepository = petRepository;
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Pet> GetPet(string id)
    {
        var pet = await _petRepository.GetPet(id);
        if (pet == null)
            throw new KeyNotFoundException("Питомец не найден");

        await UpdatePetState(pet);
        await EndSleep(pet);
        return pet;
    }

    public async Task<Pet> GetPetByUserId(string userId)
    {
        var user = await _userRepository.GetUser(userId);
        if (string.IsNullOrEmpty(user.PetID))
            throw new KeyNotFoundException("Пользователь не имеет питомца");

        return await GetPet(user.PetID);
    }

    public async Task<Pet> CreatePetForUser(string userId, string petName)
    {
        var user = await _userRepository.GetUser(userId);

        if (!string.IsNullOrEmpty(user.PetID))
            throw new InvalidOperationException("Пользователь уже имеет питомца");

        var pet = new Pet
        {
            Name = petName,
            HungerCount = 100,
            HappinessCount = 100,
            HygieneCount = 100,
            EnergyCount = 100,
            IsAlive = true,
            BirthDate = DateTime.UtcNow,
            LastSleepTime = DateTime.UtcNow,
            LastFedTime = DateTime.UtcNow,
            LastCleanedTime = DateTime.UtcNow,
            IsSleeping = false
        };

        var createdPet = await _petRepository.CreatePet(pet);

        user.PetID = createdPet.Id;
        await _userRepository.UpdateUser(userId, user);

        return createdPet;
    }

    public async Task FeedPet(string userId, string foodName)
    {
        var pet = await GetPetByUserId(userId);
        CheckIfAlive(pet);

        var user = await _userRepository.GetUser(userId);
        if (string.IsNullOrEmpty(user.InventoryId))
            throw new KeyNotFoundException("Инвентарь пользователя не найден");

        var inventory = await _inventoryRepository.RemoveItemFromInventory(user.InventoryId, foodName, 1);
        if (inventory == null)
            throw new InvalidOperationException("Inventory is null");

        pet.HungerCount = Math.Min(100, pet.HungerCount + PetActionValues.HungerOnFeed);
        pet.LastFedTime = DateTime.UtcNow;
        await _petRepository.UpdatePet(pet.Id, pet);
    }

    public async Task PlayWithPet(string userId)
    {
        var pet = await GetPetByUserId(userId);
        CheckIfAlive(pet);

        var user = await _userRepository.GetUser(userId);
        if (string.IsNullOrEmpty(user.InventoryId))
            throw new KeyNotFoundException("Инвентарь пользователя не найден");

        var inventory = await _inventoryRepository.RemoveItemFromInventory(user.InventoryId, "Toy", 1);
        if (inventory == null)
            throw new InvalidOperationException("Недостаточно игрушек в инвентаре");

        pet.HappinessCount = Math.Min(100, pet.HappinessCount + PetActionValues.HappinessOnPlay);
        pet.EnergyCount = Math.Max(0, pet.EnergyCount - PetActionValues.EnergyOnPlayCost);

        await _petRepository.UpdatePet(pet.Id, pet);
    }

    public async Task CleanPet(string userId, string CleanItemName)
    {
        var pet = await GetPetByUserId(userId);
        CheckIfAlive(pet);

        var user = await _userRepository.GetUser(userId);
        if (string.IsNullOrEmpty(user.InventoryId))
            throw new KeyNotFoundException("Инвентарь пользователя не найден");

        var inventory = await _inventoryRepository.RemoveItemFromInventory(user.InventoryId, CleanItemName, 1);
        if (inventory == null)
            throw new InvalidOperationException("Недостаточно предметов для чистки");

        pet.HygieneCount = Math.Min(100, pet.HygieneCount + PetActionValues.HygieneOnClean);
        pet.LastCleanedTime = DateTime.UtcNow;
        await _petRepository.UpdatePet(pet.Id, pet);
    }

    public async Task StartSleep(string userId)
    {
        var pet = await GetPetByUserId(userId);
        CheckIfAlive(pet);

        var user = await _userRepository.GetUser(userId);
        if (string.IsNullOrEmpty(user.InventoryId))
            throw new KeyNotFoundException("Инвентарь пользователя не найден");

        var inventory = await _inventoryRepository.RemoveItemFromInventory(user.InventoryId, "Sleep", 1);
        if (inventory == null)
            throw new InvalidOperationException("Недостаточно предметов для сна");

        if (pet.IsSleeping)
            throw new InvalidOperationException("Питомец уже спит");

        pet.IsSleeping = true;
        pet.LastSleepTime = DateTime.UtcNow;
        await _petRepository.UpdatePet(pet.Id, pet);
    }

    public async Task EndSleep(Pet pet)
    {
        CheckIfAlive(pet);

        if (!pet.IsSleeping)
            return;

        var timeSinceSleepStart = DateTime.UtcNow - pet.LastSleepTime;

        if (timeSinceSleepStart.TotalHours >= PetActionValues.HoursRequiredForSleep)
        {
            pet.EnergyCount = Math.Min(100, pet.EnergyCount + PetActionValues.EnergyOnSleep);
            pet.IsSleeping = false;
            await _petRepository.UpdatePet(pet.Id, pet);
        }
        else
        {
            throw new InvalidOperationException($"Питомцу нужно спать минимум {PetActionValues.HoursRequiredForSleep} часов. Сейчас: {timeSinceSleepStart.TotalHours:F1}ч");
        }
    }

    public async Task DeletePetForUser(string userId)
    {
        var pet = await GetPetByUserId(userId);
        await _petRepository.DeletePet(pet.Id);

        var user = await _userRepository.GetUser(userId);
        user.PetID = null;
        await _userRepository.UpdateUser(userId, user);
    }

    private void CheckIfAlive(Pet pet)
    {
        if (!pet.IsAlive)
            throw new InvalidOperationException("Питомец умер");
    }

    private async Task UpdatePetState(Pet pet)
    {
        var now = DateTime.UtcNow;


        var timeSinceLastUpdate = now - pet.LastStateUpdateTime;
        if (timeSinceLastUpdate.TotalHours < 1) 
            return;

      
        var hoursSinceFed = (now - pet.LastFedTime).TotalHours;
        if (hoursSinceFed > 6)
        {
            var effectiveHours = Math.Max(0, hoursSinceFed - pet.LastStateUpdateTime.Subtract(pet.LastFedTime).TotalHours);
            var decrease = (int)(effectiveHours / 6) * 10;
            pet.HungerCount = Math.Max(0, pet.HungerCount - decrease);
        }

 
        var hoursSinceCleaned = (now - pet.LastCleanedTime).TotalHours;
        if (hoursSinceCleaned > 1)
        {
            var effectiveHours = Math.Max(0, hoursSinceCleaned - pet.LastStateUpdateTime.Subtract(pet.LastCleanedTime).TotalHours);
            var decrease = (int)effectiveHours * 1;
            pet.HygieneCount = Math.Max(0, pet.HygieneCount - decrease);
        }

        
        var hoursSinceLastSleep = (now - pet.LastSleepTime).TotalHours;
        if (!pet.IsSleeping && hoursSinceLastSleep > PetActionValues.HoursToLoseHappinessFromNoSleep)
        {
            var effectiveHoursSinceLastSleep = Math.Max(0, hoursSinceLastSleep - pet.LastStateUpdateTime.Subtract(pet.LastSleepTime).TotalHours);
            if (effectiveHoursSinceLastSleep >= PetActionValues.HoursToLoseHappinessFromNoSleep)
            {
                pet.HappinessCount = Math.Max(0, pet.HappinessCount - PetActionValues.HappinessOnNoSleep);
                pet.LastSleepTime = now; 
            }
        }

        if (pet.HungerCount <= 0 || pet.EnergyCount <= 0)
        {
            pet.IsAlive = false;
        }

        pet.LastStateUpdateTime = now;

        await _petRepository.UpdatePet(pet.Id, pet);
    }
}