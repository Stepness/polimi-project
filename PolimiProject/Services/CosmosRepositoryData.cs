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

    public async Task UpsertFileAsync(BlobEntity file)
    {
        var existingFile = await SearchFileNameAsync(file.FileName)!;

        file.Id = existingFile is null ? Guid.NewGuid().ToString() : existingFile.Id;

        await _dataContainer.UpsertItemAsync(file, new PartitionKey(file.Id));
    }

    public async Task<BlobEntity> DownloadFileAsync(string fileName)
    {
        var file = await SearchFileNameAsync(fileName)!;

        if (file == null)
        {
            throw new FileNotFoundException();
        }
        
        return file;
    }

    public async Task<BlobEntity> RenameFileAsync(string currentFileName, string newName)
    {
        var file = await SearchFileNameAsync(currentFileName);
        
        if (file == null)
        {
            throw new FileNotFoundException();
        }

        file.FileName = newName;
        return await _dataContainer.UpsertItemAsync(file, new PartitionKey(file.Id));
    }

    public async Task<List<BlobEntity>> GetAllFilesAsync()
    {
        var result = new List<BlobEntity>();
        var files = _dataContainer.GetItemLinqQueryable<BlobEntity>().Select(x=>new
            BlobEntity
            {
                Id = x.Id,
                FileName = x.FileName,
                ContentType = x.ContentType
            }
        ).ToFeedIterator();
        
        while (files.HasMoreResults)
        {
            var response = await files.ReadNextAsync();
            result.AddRange(response.Resource);
        }

        return result;
    }

    private async Task<BlobEntity> SearchFileNameAsync(string fileName)
    {
        var query = _dataContainer.GetItemLinqQueryable<BlobEntity>()
            .Where(x => x.FileName == fileName)
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