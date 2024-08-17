using PolimiProject.Models;

namespace PolimiProject.Services;

public class InMemoryRepositoryData : IRepositoryData
{
    private List<BlobEntity> _files = new();
    
    public Task UpsertFileAsync(BlobEntity file)
    {
        _files.Add(file);
        return Task.CompletedTask;
    }

    public Task<BlobEntity> DownloadFileAsync(string fileName)
    {
        var file = _files.FirstOrDefault(x => x.FileName == fileName);
        return Task.FromResult(file);
    }

    public Task<BlobEntity> RenameFileAsync(string currentFileName, string newName)
    {
        var file = _files.FirstOrDefault(x => x.FileName == currentFileName);
        file.FileName = newName;
        return Task.FromResult(file);
    }

    public Task<List<BlobEntity>> GetAllFilesAsync()
    {
        return Task.FromResult(_files);
    }
}