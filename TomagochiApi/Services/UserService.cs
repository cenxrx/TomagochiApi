using TomagochiApi.Interfaces;
using TomagochiApi.Models;

namespace TomagochiApi.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public UserService(IUserRepository userRepository, IInventoryRepository inventoryRepository)
    {
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<User> CreateUser(User user)
    {
        if (await _userRepository.CheckIfEmailExists(user.Email))
            throw new ArgumentException("Email already exists");

        var inventory = new Inventory { CarrotCount = 0, WaterCount = 0, MeatCount = 0 };
        var createdInventory = await _inventoryRepository.CreateInventory(inventory);
        
        var newUser = new User
        {
            Email = user.Email,
            Password = user.Password, //  должно быть хеширование
            IsAdmin = user.IsAdmin,
            InventoryId = createdInventory.id
        };

        return await _userRepository.CreateUser(newUser);
    }

    public async Task<User> GetUserById(string id)
    {
        var user = await _userRepository.GetUser(id);
        return user ?? throw new KeyNotFoundException("User not found");
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);
        return user ?? throw new KeyNotFoundException("User not found");
    }

    public async Task UpdateUser(string id, UserUpdateDto userParam)
    {
        var user = await GetUserById(id);

        if (!string.IsNullOrEmpty(userParam.Email) && userParam.Email != user.Email)
        {
            if (await _userRepository.CheckIfEmailExists(userParam.Email))
                throw new ArgumentException("New email already taken");

            user.Email = userParam.Email;
        }

        if (!string.IsNullOrEmpty(userParam.Password))
            user.Password = userParam.Password; //  добавить хеширование

        await _userRepository.UpdateUser(id, user);
    }

    public async Task DeleteUser(string id)
    {
        var user = await GetUserById(id);
        await _userRepository.DeleteUser(id);
        await _inventoryRepository.DeleteInventory(user.InventoryId);
    }

    public async Task<bool> ValidateCredentials(string email, string password)
    {
        var user = await _userRepository.GetUserByEmail(email);
        return user != null && user.Password == password; //  использовать хеширование
    }
}

public class UserUpdateDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}