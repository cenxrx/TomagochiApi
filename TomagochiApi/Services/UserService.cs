using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;

namespace TomagochiApi.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPetRepository _petRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IInventoryRepository inventoryRepository,
        IPetRepository petRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
        _petRepository = petRepository;
        _configuration = configuration;
    }

    public async Task<string> AuthenticateUser(string email, string password)
    {
        var user = await _userRepository.GetUserByEmail(email);

        if (user == null || !await ValidateCredentials(email, password))
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    public async Task<User> CreateUser(UserUpdateDto userDTO)
    {
        if (userDTO == null)
            throw new NullReferenceException(nameof(userDTO));
        if (await _userRepository.CheckIfEmailExists(userDTO.Email))
            throw new ArgumentException("Email already exists");

        var user = new User
        {
            Email = userDTO.Email,
            Name = userDTO.Name,
            Password = userDTO.Password,
            IsAdmin = false
        };
        var inventory = new Inventory();
        var createdInventory = await _inventoryRepository.CreateInventory(inventory);
        user.InventoryId = createdInventory.id;

        return await _userRepository.CreateUser(user);
    }

    public async Task<User> GetUserById(string id)
    {
        var user = await _userRepository.GetUser(id);
        return user ?? throw new KeyNotFoundException("User not found");
    }

    public async Task<Inventory> GetInventoryByUserId(string userId)
    {
        var user = await GetUserById(userId);
        var inventory = await _inventoryRepository.GetInventory(user.InventoryId);

        return inventory ?? throw new KeyNotFoundException("Inventory not found");
    }

    public async Task<Pet> GetPetByUserId(string userId)
    {
        var user = await GetUserById(userId);
        var pet = await _petRepository.GetPet(user.PetID);

        return pet ?? throw new KeyNotFoundException("pet not found");
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
            user.Password = userParam.Password;

        if (!string.IsNullOrEmpty(userParam.Name))
            user.Name = userParam.Name;

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
        return user != null && user.Password == password;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("UserId", user.Id),
            new Claim("InventoryId", user.InventoryId)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class UserUpdateDto
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
}
