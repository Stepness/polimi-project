using System.Security.Cryptography;
using System.Text;
using PolimiProject.Models;
using PolimiProject.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace PolimiProject.Services;

public class CosmosRepositoryUsers : IRepositoryUsers
{
    
    private readonly Container _loginContainer;
    private ICosmosLinqQuery _cosmosLinqQuery;
    
    public CosmosRepositoryUsers(ICosmosClientFactory cosmosFactory, ICosmosLinqQuery cosmosLinqQuery)
    {
        var cosmosClient = cosmosFactory.Create();
        _loginContainer = cosmosClient.GetContainer("polimiproject", "users");
        _cosmosLinqQuery = cosmosLinqQuery;
    }
    
    public async Task<UserEntity> Authenticate(string username, string password)
    {
        var query = _loginContainer.GetItemLinqQueryable<UserEntity>()
            .Where(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                        && u.Password == HashPassword(password));
        
        var response = await _cosmosLinqQuery.ListResultAsync(query);
        
        return response.FirstOrDefault();

    }

    public async Task<List<UserEntity>> GetAllUsers()
    {
        var query = _loginContainer
            .GetItemLinqQueryable<UserEntity>();
        
        var results = await _cosmosLinqQuery.ListResultAsync(query);
        
        return results;
    }

    public async Task<AddUserResult> AddUserAsync(UserEntity user)
    {
        var existingUser = await GetUserByUsernameAsync(user.Username);
        
        if (existingUser != null)
        {
            Console.WriteLine($"User with username '{user.Username}' already exists.");

            return new AddUserResult { Result = AddUserResultType.UserAlreadyExists };
        }

        user.Id = Guid.NewGuid().ToString();
        user.Password = HashPassword(user.Password);
        
        var response = await _loginContainer.CreateItemAsync(user, new PartitionKey(user.Id));
        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            Console.WriteLine($"User with username '{user.Username}' added successfully.");
            return new AddUserResult
            {
                Result = AddUserResultType.Success, 
                User = response.Resource
            };
        }

        Console.WriteLine($"Failed to add user with username '{user.Username}'.");
        return new AddUserResult { Result = AddUserResultType.Failure };
    }
    
    public async Task<bool> UpdateUserRoleToWriter(string username)
    {
        var user = await GetUserByUsernameAsync(username);
    
        if (user == null)
        {
            Console.WriteLine($"User with ID '{username}' not found.");
            return false;
        }

        if (user.Role == Roles.Admin)
        {
            Console.WriteLine("You cant demote an admin");
            return false;
        }

        user.Role = Roles.Writer;
    
        var response = await _loginContainer.UpsertItemAsync(user);
    
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"User with ID '{username}' role updated to 'Writer'.");
            return true;
        }
    
        Console.WriteLine($"Failed to update role for user with ID '{username}'.");
        return false;
    }


    private async Task<UserEntity> GetUserByUsernameAsync(string username)
    {
        var query = _loginContainer.GetItemLinqQueryable<UserEntity>()
            .Where(u => u.Username == username);

        var response = await _cosmosLinqQuery.ListResultAsync(query);
        
        return response.FirstOrDefault();
    }
    
    private string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}