using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace PolimiProject.Controllers;

[ApiController]
[Route("[controller]")]
public class BlobController : ControllerBase
{
    private IRepositoryData _repositoryData;
    
    public BlobController(IRepositoryData repositoryData)
    {
        _repositoryData = repositoryData;
    }
    
    [HttpPost("upload")]
    [Authorize(Policy = IdentityData.WriterUserPolicy)]
    public async Task<ActionResult> Upload(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();
        
        var blob = new BlobEntity()
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Data = fileBytes
        };
        
        await _repositoryData.UpsertFileAsync(blob);
        return Ok();
    }
    
    [HttpGet("download")]
    public async Task<ActionResult<IFormFile>> Download(string fileName)
    {
        var blobEntity = await _repositoryData.DownloadFileAsync(fileName);
        return File(blobEntity.Data, blobEntity.ContentType, blobEntity.FileName);
    }
    
    [HttpPut("{fileName}/rename")]
    [Authorize(Policy = IdentityData.WriterUserPolicy)]
    public async Task<ActionResult> Rename(string fileName, string newName)
    {
        await _repositoryData.RenameFileAsync(fileName, newName);
        return Ok();
    }
    
    [HttpDelete("{fileName}/delete")]
    [Authorize(Policy = IdentityData.WriterUserPolicy)]
    public async Task<ActionResult> Delete(string fileName)
    {
        await _repositoryData.DeleteFileAsync(fileName);
        return Ok();
    }
    
    [HttpGet("files")]
    public async Task<ActionResult> GetAllFiles()
    {
        var files =await _repositoryData.GetAllFilesAsync();
        return Ok(files);
    }
}