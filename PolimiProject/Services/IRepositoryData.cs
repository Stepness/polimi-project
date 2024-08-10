using PolimiProject.Models;

namespace PolimiProject.Services;

public interface IRepositoryData
{
    Task UploadFileAsync(BlobEntity file);
    Task<BlobEntity> DownloadFileAsync(string id);
}