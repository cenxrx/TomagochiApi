using Microsoft.Extensions.Configuration;
using Moq;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;
using TomagochiApi.Services;
using Xunit;

namespace TomagochiApi.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IInventoryRepository> _inventoryRepoMock = new();
    private readonly Mock<IPetRepository> _petRepoMock = new();
     private readonly UserService _userService;

    public UserServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"JwtSettings:Key", "super-secret-key-for-testing"},
                {"JwtSettings:Issuer", "test-issuer"},
                {"JwtSettings:Audience", "test-audience"},
                {"JwtSettings:ExpiryMinutes", "60"}
            })
            .Build();

        _userService = new UserService(
            _userRepoMock.Object,
            _inventoryRepoMock.Object,
            _petRepoMock.Object,
            configuration);
    }
    
    

    [Fact]
    public async Task UpdateUser_WithNewEmail_ShouldUpdateUser()
    {
        var existingUser = new User
        {
            Id = "123",
            Email = "old@example.com",
            Name = "Old Name",
            Password = "oldpass"
        };

        var updateDto = new UserUpdateDto
        {
            Email = "new@example.com",
            Name = "New Name",
            Password = "newpass"
        };

        _userRepoMock.Setup(x => x.GetUser(It.IsAny<string>()))
            .ReturnsAsync(existingUser);
        
        _userRepoMock.Setup(x => x.CheckIfEmailExists(It.IsAny<string>()))
            .ReturnsAsync(false);

        await _userService.UpdateUser("123", updateDto);

        _userRepoMock.Verify(x => x.UpdateUser("123", It.Is<User>(u => 
            u.Email == "new@example.com" && 
            u.Name == "New Name" && 
            u.Password == "newpass")), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUserAndInventory()
    {
        var user = new User
        {
            Id = "123",
            InventoryId = "inv123"
        };

        _userRepoMock.Setup(x => x.GetUser(It.IsAny<string>()))
            .ReturnsAsync(user);

        await _userService.DeleteUser("123");

        _userRepoMock.Verify(x => x.DeleteUser("123"), Times.Once);
        _inventoryRepoMock.Verify(x => x.DeleteInventory("inv123"), Times.Once);
    }

    [Fact]
    public async Task CreateUser_ExistingEmail_ThrowsException()
    {
        var userDto = new UserUpdateDto { Email = "existing@example.com" };
        
        _userRepoMock.Setup(x => x.CheckIfEmailExists(It.IsAny<string>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUser(userDto));
    }
}