using TomagochiApi.Models;

namespace TomagochiApi.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUser(User user);
    Task<User> GetUser(string id);
    Task<User> GetUserByEmail(string email);
    Task UpdateUser(string id, User user);
    Task DeleteUser(string id);
    Task<bool> CheckIfEmailExists(string email);
}