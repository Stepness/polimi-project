using System.Reflection.Metadata;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using PolimiProject.Models;
using PolimiProject.Services;
using FluentAssertions;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Cosmos.Linq;
using NSubstitute.ReturnsExtensions;

namespace UnitTests.Services;

public class CosmosRepositoryDataTests
{
    private readonly Container _mockContainer;
    private readonly CosmosClient _mockCosmosClient;

    public CosmosRepositoryDataTests()
    {
        _mockContainer = Substitute.For<Container>();
        _mockCosmosClient = Substitute.For<CosmosClient>();

        _mockCosmosClient
            .GetContainer(Arg.Any<string>(), Arg.Any<string>())
            .Returns(_mockContainer);
    }

    [Fact]
    public async Task WhenUploadFile_ShouldCallCreateItemAsync()
    {
        
        var sut = new CosmosRepositoryData(_mockCosmosClient);
        
        var blobEntity = new BlobEntity();

        await sut.UpsertFileAsync(blobEntity);
        
        await _mockContainer.Received(1).CreateItemAsync(
            blobEntity,
            Arg.Is<PartitionKey>(pk => pk == new PartitionKey(blobEntity.Id)));
    }
    
    [Fact]
    public async Task WhenDownloadFile_ShouldCallCreateItemAsync()
    {
        var data = new List<BlobEntity>
        {
            new BlobEntity{Id = "abc"},
        }.AsQueryable();

        // var documents = new List<BlobEntity>() { new BlobEntity() };
        // var mockFeedResponse = Substitute.For<FeedResponse<BlobEntity>>();
        // mockFeedResponse.GetEnumerator().Returns(documents.GetEnumerator());

        _mockContainer.GetItemLinqQueryable<BlobEntity>().Returns(data);
        
        var sut = new CosmosRepositoryData(_mockCosmosClient);
        
        await sut.DownloadFileAsync("abc");
        
        _mockContainer.Received(1).GetItemLinqQueryable<BlobEntity>();
        
    }
    
    [Fact]
    public async Task MyTestMethod()
    {
        List<BlobEntity> results = new List<BlobEntity>
        {
            new BlobEntity { Id = "abc"}
        };

        var mockedResponse = Substitute.For<FeedResponse<BlobEntity>>();
        mockedResponse.Resource.Returns(results);

        var mockedIterator = Substitute.For<FeedIterator<BlobEntity>>();
        mockedIterator.ReadNextAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockedResponse));
        mockedIterator.HasMoreResults.Returns(true, false);

        while (mockedIterator.HasMoreResults)
        {
            FeedResponse<BlobEntity> feedResponse = await mockedIterator.ReadNextAsync();
            feedResponse.Resource.Should().BeEquivalentTo(results);
        }

        mockedIterator.Received(1).ReadNextAsync(Arg.Any<CancellationToken>());
        
        
        var data = new List<BlobEntity>
        {
            new BlobEntity{Id = "abc"},
        }.AsQueryable();

        _mockContainer.GetItemLinqQueryable<BlobEntity>().Returns(data);
        
        var sut = new CosmosRepositoryData(_mockCosmosClient);
        
        await sut.DownloadFileAsync("abc");
        
        _mockContainer.Received(1).GetItemLinqQueryable<BlobEntity>();
        //#https://stackoverflow.com/questions/62340160/error-while-writing-unit-test-for-cosmos-client-can-not-convert-listmodel-to
    }
}