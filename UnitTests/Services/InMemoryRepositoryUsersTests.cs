using AutoFixture;
using FluentAssertions;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace UnitTests.Services;

public class InMemoryRepositoryUsersTests
{
     private readonly Fixture _fixture;
    private readonly InMemoryRepositoryUsers _repository;

    public InMemoryRepositoryUsersTests()
    {
        _fixture = new Fixture();
        _repository = new InMemoryRepositoryUsers();
    }

    [Fact]
    public async Task WhenValidAuthentication_ShouldReturnUser()
    {
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();

        await _repository.AddUserAsync(new UserEntity() { Username = username, Password = password });
        var user = await _repository.Authenticate(username, password);

        user.Should().NotBeNull();
        user.Username.Should().Be(username);
        user.Password.Should().Be(password);
    }

    [Fact]
    public async Task WhenInvalidAuthentication_ShouldReturnNull()
    {
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();

        var user = await _repository.Authenticate(username, password);

        user.Should().BeNull();
    }

    [Fact]
    public async Task WhenGetAllUsers_ShouldReturnAllUsers()
    {
        var users = _fixture.CreateMany<UserEntity>(3).ToList();
        foreach (var user in users)
        {
            await _repository.AddUserAsync(user);
        }

        var allUsers = await _repository.GetAllUsers();

        allUsers.Should().Contain(users);
    }

    [Fact]
    public async Task WhenAddUser_ShouldAddUser()
    {
        var user = _fixture.Create<UserEntity>();

        var result = await _repository.AddUserAsync(user);
        var allUsers = await _repository.GetAllUsers();

        result.Result.Should().Be(AddUserResultType.Success);
        result.User.Should().Be(user);
        allUsers.Should().Contain(user);
    }

    [Fact]
    public async Task WhenUpdateUserRoleToWriter_ShouldUpdateRole()
    {
        var user = _fixture.Create<UserEntity>();
        await _repository.AddUserAsync(user);
        var newRole = Roles.Writer;

        await _repository.UpdateUserRoleToWriter(user.Username);
        var updatedUser = (await _repository.GetAllUsers()).FirstOrDefault(u => u.Username == user.Username);

        updatedUser.Should().NotBeNull();
        updatedUser.Role.Should().Be(newRole);
    }
}