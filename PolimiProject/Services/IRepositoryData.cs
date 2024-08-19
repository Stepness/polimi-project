using System.Reflection.Metadata;
using PolimiProject.Models;

namespace PolimiProject.Services;

public interface IRepositoryData
{
    Task UpsertFileAsync(BlobEntity file);
    Task<BlobEntity> DownloadFileAsync(string fileName);
    Task<BlobEntity> RenameFileAsync(string currentFileName, string newName);
    Task<BlobEntity> DeleteFileAsync(string fileName);
    Task<List<BlobEntity>> GetAllFilesAsync();
}