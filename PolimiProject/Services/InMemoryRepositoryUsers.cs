using Microsoft.Azure.Cosmos;
using PolimiProject.Models;
using PolimiProject.Settings;

namespace PolimiProject.Services;

public class InMemoryRepositoryUsers : IRepositoryUsers
{
    public InMemoryRepositoryUsers()
    {
        _users = new();
        _users.Add(new UserEntity { Username = "admin", Password = "admin", Role = Roles.Admin });
    }

    private List<UserEntity> _users;
    public Task<UserEntity> Authenticate(string username, string password)
    {
        var user = _users.FirstOrDefault(x => x.Username == username && x.Password == password);
        return Task.FromResult(user);
    }

    public Task<List<UserEntity>> GetAllUsers()
    {
        return Task.FromResult(_users);
    }

    public Task<AddUserResult> AddUserAsync(UserEntity user)
    {
        _users.Add(user);
        return Task.FromResult(new AddUserResult{Result = AddUserResultType.Success, User = user});
    }

    public Task<bool> UpdateUserRoleToWriter(string username)
    {
        var user = _users.FirstOrDefault(x => x.Username == username);
        user.Role = Roles.Writer;
        return Task.FromResult(true);
    }
}