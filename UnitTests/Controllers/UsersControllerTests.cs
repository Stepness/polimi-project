using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PolimiProject.Controllers;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly IRepositoryUsers _repositoryUsersMock;
    private readonly UsersController sut;

    public UsersControllerTests()
    {
        _repositoryUsersMock = Substitute.For<IRepositoryUsers>();
        sut = new UsersController(_repositoryUsersMock);
    }

    [Fact]
    public void WhenValidateUser_ShouldReturnOk()
    {
        var result = sut.Validate();
        result.Should().BeOfType<OkResult>();
    } 
    

    [Fact]
    public async Task WhenRegister_ShouldAddNewUser()
    {
        var signDto = new SignDto { Username = "username", Password = "password" };
        var userEntity = new UserEntity { Username = signDto.Username, Password = signDto.Password, Role = Roles.Guest };
        
        var addUserResult = new AddUserResult
        {
            Result = AddUserResultType.Success,
            User = userEntity
        };
        
        _repositoryUsersMock.AddUserAsync(Arg.Any<UserEntity>()).Returns(addUserResult);
        
        var result = await sut.Register(signDto);

        await _repositoryUsersMock.Received(1).AddUserAsync(Arg.Is<UserEntity>(u =>
            u.Username == signDto.Username &&
            u.Password == signDto.Password &&
            u.Role == Roles.Guest
        ));

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value?.ToString().Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task WhenRegisterAlreadyExistingUser_ShouldReturnConflict()
    {
        var addUserResult = new AddUserResult
        {
            Result = AddUserResultType.UserAlreadyExists,
            User = new UserEntity()
        };
        
        _repositoryUsersMock.AddUserAsync(Arg.Any<UserEntity>()).Returns(addUserResult);
        
        var result = await sut.Register(new SignDto());

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task WhenGetAllUsers_ShouldReturnAllUsersAndRoles()
    {
        var users = new List<UserEntity>
        {
            new() { Username = "user1", Role = Roles.Admin },
            new() { Username = "user2", Role = Roles.Guest },
        };

        var expectedUsers = users.Select(x => new { x.Username, x.Role }).ToList();
        _repositoryUsersMock.GetAllUsers().Returns(users);

        var result = await sut.GetAll();

        await _repositoryUsersMock.Received(1).GetAllUsers();
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedUsers);
    }
    
    [Fact]
    public async Task WhenLoginSuccessful_ReturnToken()
    {
        var signDto = new SignDto { Username = "username", Password = "password" };
        var userEntity = new UserEntity { Username = signDto.Username, Role = Roles.Viewer };

        _repositoryUsersMock.Authenticate(signDto.Username, signDto.Password)!.Returns(Task.FromResult(userEntity));

        var result = await sut.Login(signDto);

        await _repositoryUsersMock.Received(1).Authenticate(signDto.Username, signDto.Password);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task WhenLoginUnauthorized_ReturnUnauthorized()
    {
        _repositoryUsersMock.Authenticate(Arg.Any<string>(), Arg.Any<string>())!.Returns(Task.FromResult<UserEntity>(null!));

        var result = await sut.Login(new SignDto());

        await _repositoryUsersMock.Received(1).Authenticate(Arg.Any<string>(), Arg.Any<string>());

        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Username or password is incorrect" });

    }
}