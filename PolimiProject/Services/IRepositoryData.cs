using System.Reflection.Metadata;
using PolimiProject.Models;

namespace PolimiProject.Services;

public interface IRepositoryData
{
    Task UpsertFileAsync(BlobEntity file);
    Task<BlobEntity> DownloadFileAsync(string id);
    Task<BlobEntity> RenameFileAsync(string currentFileName, string newName);
}