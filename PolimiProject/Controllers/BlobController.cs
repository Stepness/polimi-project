using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolimiProject.Identity;
using PolimiProject.Models;
using PolimiProject.Services;

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
    public async Task<ActionResult> Upload(IFormFile model)
    {
        using var ms = new MemoryStream();
        await model.CopyToAsync(ms);
        var fileBytes = ms.ToArray();
        
        var blob = new BlobEntity()
        {
            FileName = model.FileName,
            ContentDisposition = model.ContentDisposition,
            ContentType = model.ContentType,
            Data = fileBytes
        };
        
        await _repositoryData.UploadFileAsync(blob);
        return Ok();
    }
    
    [HttpGet("download")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IFormFile>> Download(string id)
    {
        var blobEntity = await _repositoryData.DownloadFileAsync(id);
        return File(blobEntity.Data, blobEntity.ContentType, blobEntity.FileName);
    }
}