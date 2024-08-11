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

        await sut.UploadFileAsync(blobEntity);
        
        await _mockContainer.Received(1).CreateItemAsync(
            blobEntity,
            Arg.Is<PartitionKey>(pk => pk == new PartitionKey(blobEntity.Id)));
    }
    
    // [Fact]
    // public async Task WhenDownloadFile_ShouldCallCreateItemAsync()
    // {
    //     _mockContainer.GetItemLinqQueryable<BlobEntity>()
    //         .Where(u => u.Id == "abc")
    //         .Take(1);
    //     
    //     var sut = new CosmosRepositoryData(_mockCosmosClient);
    //     
    //     await sut.DownloadFileAsync("abc");
    //
    //     _mockContainer.Received(1).GetItemLinqQueryable<BlobEntity>();
    // }
}