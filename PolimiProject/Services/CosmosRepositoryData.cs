using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using PolimiProject.Models;

namespace PolimiProject.Services;

public class CosmosRepositoryData : IRepositoryData
{
    private readonly ICosmosLinqQuery _cosmosLinqQuery;
    private readonly Container _dataContainer;

    public CosmosRepositoryData(ICosmosClientFactory cosmosFactory, ICosmosLinqQuery cosmosLinqQuery)
    {
        var cosmosClient = cosmosFactory.Create();
        _cosmosLinqQuery = cosmosLinqQuery;
        _dataContainer = cosmosClient.GetContainer("polimiproject", "data");
    }

    public async Task UpsertFileAsync(BlobEntity file)
    {
        var existingFile = await SearchFileNameAsync(file.FileName)!;

        file.Id = existingFile is null ? Guid.NewGuid().ToString() : existingFile.Id;

        var response = await _dataContainer.UpsertItemAsync(file, new PartitionKey(file.Id));
        
        
        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception($"Failed upserting File: {file.FileName}");
        }
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
        var newNameFile = await SearchFileNameAsync(newName);

        if (newNameFile != null)
        {
            throw new Exception("New name not valid. File already exists");
        }
            
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
        
        var query = _dataContainer.GetItemLinqQueryable<BlobEntity>().Select(x => new
            BlobEntity
            {
                Id = x.Id,
                FileName = x.FileName,
                ContentType = x.ContentType
            });
        
        var result = await _cosmosLinqQuery.ListResultAsync(query);

        return result;
    }

    private async Task<BlobEntity> SearchFileNameAsync(string fileName)
    {
        var query = _dataContainer.GetItemLinqQueryable<BlobEntity>()
            .Where(x => x.FileName == fileName);

        var result = await _cosmosLinqQuery.ListResultAsync(query);

        return result.FirstOrDefault();
    }
}