using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PolimiProject.Models;
using PolimiProject.Services;

namespace UnitTests.Services;

public class CosmosRepositoryDataTests
{
    private readonly Fixture _fixture;
    private readonly Container _mockContainer;
    private readonly ICosmosClientFactory _mockFactory;
    private readonly ICosmosLinqQuery _linqQuery;

    public CosmosRepositoryDataTests()
    {
        _fixture = new Fixture();
        _mockContainer = Substitute.For<Container>();
        var mockClient = Substitute.For<CosmosClient>();
        _mockFactory = Substitute.For<ICosmosClientFactory>();
        _linqQuery = Substitute.For<ICosmosLinqQuery>();
        
        _mockContainer.GetItemLinqQueryable<BlobEntity>().Returns(new List<BlobEntity>().AsQueryable());

        mockClient.GetContainer(Arg.Any<string>(), Arg.Any<string>())
            .Returns(_mockContainer);
        _mockFactory.Create().Returns(mockClient);
    }
    
    [Fact]
    public async Task WhenUpsertNewFile_ShouldGenerateNewIdAndUpsertFile()
    {
        var response = Substitute.For<ItemResponse<BlobEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.OK);
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>())
            .Returns([]);
        
        _mockContainer.UpsertItemAsync(Arg.Any<BlobEntity>(), Arg.Any<PartitionKey>()).Returns(response);
        
        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        var file = _fixture.Create<BlobEntity>();
        file.Id = "";

        await sut.UpsertFileAsync(file);

        file.Id.Should().NotBeNullOrEmpty();
        await _mockContainer.Received(1)
            .UpsertItemAsync(file, Arg.Is<PartitionKey>(pk => !string.IsNullOrEmpty(pk.ToString())));
    }

    [Fact]
    public async Task WhenUpsertExistingFile_ShouldUseExistingIdAndUpsertFile()
    {
        var existingFileName = _fixture.Create<string>();
        
        _fixture.Customize<BlobEntity>(c => 
            c.With(addr => addr.FileName, existingFileName));
        
        var existingFile = _fixture.Create<BlobEntity>();
        
        var newFile = _fixture.Create<BlobEntity>();
        newFile.Id = "";
        
        var response = Substitute.For<ItemResponse<BlobEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.OK);
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>())
            .Returns([existingFile]);
        
        _mockContainer.UpsertItemAsync(Arg.Any<BlobEntity>(), Arg.Any<PartitionKey>()).Returns(response);
        
        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        
        await sut.UpsertFileAsync(newFile);

        await _mockContainer.Received(1)
            .UpsertItemAsync(Arg.Is<BlobEntity>(x=>x.Id == existingFile.Id && x.FileName == existingFile.FileName),
                Arg.Any<PartitionKey>());
    }
    
    [Fact]
    public async Task WhenUpsertFileFails_ShouldThrowException()
    {
        var response = Substitute.For<ItemResponse<BlobEntity>>();
        response.StatusCode.Returns(System.Net.HttpStatusCode.InternalServerError);
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>())
            .Returns([]);
        
        _mockContainer.UpsertItemAsync(Arg.Any<BlobEntity>(), Arg.Any<PartitionKey>()).Returns(response);
        
        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        var file = _fixture.Create<BlobEntity>();

        var act = () => sut.UpsertFileAsync(file);
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task WhenDownloadFilename_ShouldReturnFile()
    {
        var expectedFile = _fixture.Create<BlobEntity>();
        
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>()).Returns([expectedFile]);
        
        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);

        var result = await sut.DownloadFileAsync(expectedFile.FileName);
        result.Should().Be(expectedFile);
    }
    
    [Fact]
    public async Task WhenDownloadNotExistingFilename_ShouldThrowException()
    {
        var fileName = _fixture.Create<string>();
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>()).Returns([]);
        
        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);

        var act= () => sut.DownloadFileAsync(fileName);
        await act.Should().ThrowAsync<FileNotFoundException>();
    }
    
    [Fact]
    public async Task WhenRenameFilename_ShouldThrowException()
    {
        var fileName = _fixture.Create<string>();
        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>()).Returns([]);
        
        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);

        var act= () => sut.DownloadFileAsync(fileName);
        await act.Should().ThrowAsync<FileNotFoundException>();
    }
    
    [Fact]
    public async Task WhenRenameFileWithAlreadyExistingName_ShouldThrowException()
    {
        var currentFileName = _fixture.Create<string>();
        var newName = _fixture.Create<string>();
        var existingFile = _fixture.Create<BlobEntity>();

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>())
            .Returns([existingFile]);

        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        var act = async () => await sut.RenameFileAsync(currentFileName, newName);

        await act.Should().ThrowAsync<Exception>();

        await _linqQuery.Received(1).ListResultAsync(Arg.Any<IQueryable<BlobEntity>>());
    }
    
    [Fact]
    public async Task WhenRenameNotExistingFile_ShouldThrowException()
    {
        var currentFileName = _fixture.Create<string>();
        var newName = _fixture.Create<string>();

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>())
            .Returns([]);

        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        var act = async () => await sut.RenameFileAsync(currentFileName, newName);

        await act.Should().ThrowAsync<FileNotFoundException>();
        await _linqQuery.Received(2).ListResultAsync(Arg.Any<IQueryable<BlobEntity>>());
    }
    
    [Fact]
    public async Task WhenRenameFileName_ShouldReturnFile()
    {
        var newName = _fixture.Create<string>();
        var file = _fixture.Create<BlobEntity>();

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>())
            .Returns(x=>[],
                x=>[file]);

        var response = Substitute.For<ItemResponse<BlobEntity>>();
        response.Resource.Returns(file);

        _mockContainer.UpsertItemAsync(file, Arg.Any<PartitionKey>()).Returns(response);

        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        var result = await sut.RenameFileAsync(file.FileName, newName);

        result.FileName.Should().Be(newName);

        await _linqQuery.Received(2).ListResultAsync(Arg.Any<IQueryable<BlobEntity>>());
        await _mockContainer.Received(1).UpsertItemAsync(file, Arg.Any<PartitionKey>());
    }
    
    [Fact]
    public async Task WhenGetAllFiles_ShouldReturnFilesList()
    {
        var expectedFiles = _fixture.CreateMany<BlobEntity>().ToList();

        _linqQuery.ListResultAsync(Arg.Any<IQueryable<BlobEntity>>()).Returns(expectedFiles);

        var sut = new CosmosRepositoryData(_mockFactory, _linqQuery);
        var result = await sut.GetAllFilesAsync();

        result.Should().BeEquivalentTo(expectedFiles);
        await _linqQuery.Received(1).ListResultAsync(Arg.Any<IQueryable<BlobEntity>>());
    }
}