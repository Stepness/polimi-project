using PolimiProject.Models;

namespace PolimiProject.Services;

public interface IRepositoryUsers
{
    Task<UserEntity> Authenticate(string username, string password);
    Task<List<UserEntity>> GetAllUsers();
    Task<AddUserResult> AddUserAsync(UserEntity user);
    Task<bool> UpdateUserRoleToWriter(string username);
}