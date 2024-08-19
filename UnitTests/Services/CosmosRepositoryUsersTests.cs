using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace UnitTests.Services;

public class CosmosRepositoryUsersTests
{
    private readonly Container _mockContainer;
    private readonly ICosmosClientFactory _mockFactory;
    private readonly Fixture _fixture;
    private readonly ICosmosLinqQuery _linqQuery;
    
    public CosmosRepositoryUsersTests()
    {
        _fixture = new Fixture();
        _mockContainer = Substitute.For<Container>();
        var mockClient = Substitute.For<CosmosClient>();
        _mockFactory = Substitute.For<ICosmosClientFactory>();
        _linqQuery = Substitute.For<ICosmosLinqQuery>();

        mockClient.GetContainer(Arg.Any<string>(), Arg.Any<string>())
            .Returns(_mockContainer);
        _mockFactory.Create().Returns(mockClient);
    }

    [Fact]
    public async Task WhenGettingAllUsers_ShouldReturnAllUsers()
    {
        var expectedUsers = _fixture.Create<List<UserEntity>>();
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>())
            .Returns(expectedUsers);
        
        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);
        var result = await sut.GetAllUsers();
        
        _mockContainer.GetItemLinqQueryable<UserEntity>().Received(1);
        result.Should().Contain(expectedUsers);
    }
    
    [Fact]
    public async Task WhenAuthenticating_ShouldReturnUser()
    {
        var users = new List<UserEntity>();
        var expectedUser = _fixture.Create<UserEntity>();
        users.Add(expectedUser);
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>())
            .Returns(users);

        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(users.AsQueryable());
        
        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.Authenticate(expectedUser.Username, expectedUser.Password);

        result.Password.Should().Be(expectedUser.Password);
        result.Username.Should().Be(expectedUser.Username);
    }
    
    
    
    [Fact]
    public async Task WhenAddExistingUser_ShouldReturnUserAlreadyExists()
    {
        var existingUser = _fixture.Create<UserEntity>();
        var existingUsers = new List<UserEntity> { existingUser }; 
        var newUser = _fixture.Build<UserEntity>().With(u => u.Username, existingUser.Username).Create();

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>())
            .Returns(existingUsers);
        
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());

        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.AddUserAsync(newUser);

        result.Result.Should().Be(AddUserResultType.UserAlreadyExists);
        await _mockContainer.DidNotReceiveWithAnyArgs().CreateItemAsync<UserEntity>(default!);
    }

    [Fact]
    public async Task WhenAddUserAsync_ShouldReturnSuccess()
    {
        var newUser = _fixture.Create<UserEntity>();
        var users = new List<UserEntity>();
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>()).Returns(users);
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());
        
        var response = Substitute.For<ItemResponse<UserEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.Created);
        response.Resource.Returns(newUser);

        _mockContainer.CreateItemAsync(Arg.Any<UserEntity>(), Arg.Any<PartitionKey>())
            .Returns(response);

        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.AddUserAsync(newUser);

        result.Result.Should().Be(AddUserResultType.Success);
        result.User.Should().Be(newUser);
        await _mockContainer.Received(1).CreateItemAsync(Arg.Any<UserEntity>(), Arg.Any<PartitionKey>());
    }
    
    [Fact]
    public async Task WhenFailAddUserAsync_ShouldReturnFailure()
    {
        var newUser = _fixture.Create<UserEntity>();

        var linqQuery = Substitute.For<ICosmosLinqQuery>();
        linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>()).Returns([]);
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());
        
        var response = Substitute.For<ItemResponse<UserEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.BadRequest);

        _mockContainer.CreateItemAsync(Arg.Any<UserEntity>(), Arg.Any<PartitionKey>())
            .Returns(response);

        var sut = new CosmosRepositoryUsers(_mockFactory, linqQuery);

        var result = await sut.AddUserAsync(newUser);

        result.Result.Should().Be(AddUserResultType.Failure);
        await _mockContainer.Received(1).CreateItemAsync(Arg.Any<UserEntity>(), Arg.Any<PartitionKey>());
    }

    [Fact]
    public async Task WhenUpdateUnexistingUserRole_ShouldReturnFalse()
    {
        var username = _fixture.Create<string>();
        var users = new List<UserEntity>();

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>()).Returns(users);
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());

        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.UpdateUserRoleToWriter(username);

        result.Should().BeFalse();
        await _mockContainer.DidNotReceiveWithAnyArgs().UpsertItemAsync<UserEntity>(default!);
    }

    [Fact]
    public async Task WhenUpdateAdminRole_ShouldReturnFalse()
    {
        var adminUser = _fixture.Build<UserEntity>().With(u => u.Role, Roles.Admin).Create();
        var users = new List<UserEntity>() { adminUser };
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>()).Returns(users);
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());

        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.UpdateUserRoleToWriter(adminUser.Username);

        result.Should().BeFalse();
        await _mockContainer.DidNotReceiveWithAnyArgs().UpsertItemAsync<UserEntity>(default!);
    }

    [Fact]
    public async Task WhenUpdateUserRole_ShouldReturnTrue()
    {
        var user = _fixture.Build<UserEntity>().With(u => u.Role, Roles.Reader).Create();
        var users = new List<UserEntity> { user };

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>()).Returns(users);
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());

        var response = Substitute.For<ItemResponse<UserEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.OK);

        _mockContainer.UpsertItemAsync(Arg.Any<UserEntity>())
            .Returns(response);

        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.UpdateUserRoleToWriter(user.Username);

        result.Should().BeTrue();
        await _mockContainer.Received(1).UpsertItemAsync(Arg.Is<UserEntity>(u => u.Role == Roles.Writer));
    }

    [Fact]
    public async Task WhenUpdateUserRoleFails_ShouldReturnFalse()
    {
        var user = _fixture.Build<UserEntity>().With(u => u.Role, Roles.Reader).Create();
        var users = new List<UserEntity> { user };

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<UserEntity>>()).Returns(users);
        _mockContainer.GetItemLinqQueryable<UserEntity>().Returns(new List<UserEntity>().AsQueryable());

        var response = Substitute.For<ItemResponse<UserEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.BadRequest);

        _mockContainer.UpsertItemAsync(Arg.Any<UserEntity>())
            .Returns(response);

        var sut = new CosmosRepositoryUsers(_mockFactory, _linqQuery);

        var result = await sut.UpdateUserRoleToWriter(user.Username);

        result.Should().BeFalse();
        await _mockContainer.Received(1).UpsertItemAsync(Arg.Any<UserEntity>());
    }
}