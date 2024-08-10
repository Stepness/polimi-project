using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using PolimiProject.Models;

namespace PolimiProject.Services;

public class CosmosRepositoryData : IRepositoryData
{
    private readonly Container _dataContainer;

    public CosmosRepositoryData(CosmosClient cosmosClient)
    {
        _dataContainer = cosmosClient.GetContainer("polimiproject", "data");
    }
    
    public async Task UploadFileAsync(BlobEntity file)
    {
        file.Id = Guid.NewGuid().ToString();
        await _dataContainer.CreateItemAsync(file, new PartitionKey(file.Id));
    }

    public async Task<BlobEntity> DownloadFileAsync(string id)
    {
        var query = _dataContainer.GetItemLinqQueryable<BlobEntity>()
            .Where(u => u.Id == id)
            .Take(1)
            .ToFeedIterator();

        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }
}