using MongoDB.Driver;
using TomagochiApi.Interfaces;
using TomagochiApi.Models;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;

    public UserRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("tomagochidb");
        _userCollection = database.GetCollection<User>("User");
    }

    public async Task<User> CreateUser(User user)
    {
        await _userCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<User> GetUser(string id)
    {
        return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User> GetUserByEmail(string email)
    {
        return await _userCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task UpdateUser(string id, User user)
    {
        await _userCollection.ReplaceOneAsync(u => u.Id == id, user);
    }

    public async Task DeleteUser(string id)
    {
        await _userCollection.DeleteOneAsync(u => u.Id == id);
    }

    public async Task<bool> CheckIfEmailExists(string email)
    {
        return await _userCollection.Find(u => u.Email == email).AnyAsync();
    }
}