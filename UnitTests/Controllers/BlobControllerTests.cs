using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PolimiProject.Controllers;
using PolimiProject.Models;
using PolimiProject.Services;

namespace UnitTests.Controllers;

public class BlobControllerTests
{
    private readonly IRepositoryData _repositoryDataMock;
    private readonly BlobController sut;
    private readonly Fixture _fixture;

    public BlobControllerTests()
    {
        _fixture = new Fixture();
        _repositoryDataMock = Substitute.For<IRepositoryData>();
        sut = new BlobController(_repositoryDataMock);
    }

    [Fact]
    public async Task WhenUpload_Controller_ShouldReturnOk()
    {
        var fileMock = Substitute.For<IFormFile>();
        var ms = new MemoryStream();

        fileMock.OpenReadStream().Returns(ms);
        fileMock.FileName.Returns("test.txt");
        fileMock.ContentType.Returns("text/plain");
        
        var result = await sut.Upload(fileMock);

        await _repositoryDataMock.Received(1).UpsertFileAsync(Arg.Is<BlobEntity>(b => 
            b.FileName == "test.txt" &&
            b.ContentType == "text/plain"
        ));

        result.Should().BeOfType<OkResult>();
    }
    
    [Fact]
    public async Task WhenDownload_Controller_ShouldReturnFile()
    {
        _fixture.Customize<BlobEntity>(x => 
            x.With(entity => entity.ContentType, "text/plain"));
        
        var expectedEntity = _fixture.Create<BlobEntity>();
        
        _repositoryDataMock.DownloadFileAsync(Arg.Any<string>()).Returns(expectedEntity);

        var result = await sut.Download(expectedEntity.FileName);

        await _repositoryDataMock.Received(1).DownloadFileAsync(expectedEntity.FileName);
        result.Result.As<FileContentResult>().ContentType.Should().Be(expectedEntity.ContentType);
        result.Result.As<FileContentResult>().FileDownloadName.Should().Be(expectedEntity.FileName);
    }
}