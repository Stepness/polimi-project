using AutoFixture;
using FluentAssertions;
using PolimiProject.Models;
using PolimiProject.Services;

namespace UnitTests.Services;

public class InMemoryRepositoryDataTests
{
    
    private readonly IFixture _fixture;
    private readonly InMemoryRepositoryData sut;

    public InMemoryRepositoryDataTests()
    {
        _fixture = new Fixture();
        sut = new InMemoryRepositoryData();
    }

    [Fact]
    public async Task WhenUpsertFile_ShouldAddFile()
    {
        var file = _fixture.Create<BlobEntity>();

        await sut.UpsertFileAsync(file);
        var files = await sut.GetAllFilesAsync();

        files.Should().Contain(file);
    }

    [Fact]
    public async Task WhenDownloadExistFile_ShouldReturnFile()
    {
        var file = _fixture.Create<BlobEntity>();
        await sut.UpsertFileAsync(file);

        var result = await sut.DownloadFileAsync(file.FileName);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(file);
    }

    [Fact]
    public async Task WhenDownloadFileNotExists_ShouldReturnNull()
    {
        var result = await sut.DownloadFileAsync("nonexistent.txt");

        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenRenameFile_ShouldRenameFile()
    {
        var file = _fixture.Create<BlobEntity>();
        await sut.UpsertFileAsync(file);
        var newFileName = "newname.txt";

        var result = await sut.RenameFileAsync(file.FileName, newFileName);
        var allFiles = await sut.GetAllFilesAsync();

        result.Should().NotBeNull();
        result.FileName.Should().Be(newFileName);
        allFiles.Should().ContainSingle(f => f.FileName == newFileName);
    }
    
    [Fact]
    public async Task WhenDeleteFile_ShouldDeleteFile()
    {
        var files = _fixture.CreateMany<BlobEntity>().ToList();
        foreach (var file in files)
        {
            await sut.UpsertFileAsync(file);
        }

        await sut.DeleteFileAsync(files[0].FileName);
        var allFiles = await sut.GetAllFilesAsync();

        allFiles.Should().NotContain(files[0]);
    }

    [Fact]
    public async Task WhenGetAllFiles_ShouldReturnAllFiles()
    {
        var files = _fixture.CreateMany<BlobEntity>(5).ToList();
        foreach (var file in files)
        {
            await sut.UpsertFileAsync(file);
        }

        var result = await sut.GetAllFilesAsync();

        result.Should().BeEquivalentTo(files);
    }
}