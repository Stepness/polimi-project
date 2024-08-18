using AutoFixture;
using FluentAssertions;
using PolimiProject.Models;
using PolimiProject.Services;

namespace UnitTests.Services;

public class InMemoryRepositoryDataTests
{
    
    private readonly IFixture _fixture;
    private readonly InMemoryRepositoryData _repository;

    public InMemoryRepositoryDataTests()
    {
        _fixture = new Fixture();
        _repository = new InMemoryRepositoryData();
    }

    [Fact]
    public async Task WhenUpsertFile_ShouldAddFile()
    {
        var file = _fixture.Create<BlobEntity>();

        await _repository.UpsertFileAsync(file);
        var files = await _repository.GetAllFilesAsync();

        files.Should().Contain(file);
    }

    [Fact]
    public async Task WhenDownloadExistFile_ShouldReturnFile()
    {
        var file = _fixture.Create<BlobEntity>();
        await _repository.UpsertFileAsync(file);

        var result = await _repository.DownloadFileAsync(file.FileName);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(file);
    }

    [Fact]
    public async Task WhenDownloadFileNotExists_ShouldReturnNull()
    {
        var result = await _repository.DownloadFileAsync("nonexistent.txt");

        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenRenameFile_ShouldRenameFile()
    {
        var file = _fixture.Create<BlobEntity>();
        await _repository.UpsertFileAsync(file);
        var newFileName = "newname.txt";

        var result = await _repository.RenameFileAsync(file.FileName, newFileName);
        var allFiles = await _repository.GetAllFilesAsync();

        result.Should().NotBeNull();
        result.FileName.Should().Be(newFileName);
        allFiles.Should().ContainSingle(f => f.FileName == newFileName);
    }

    [Fact]
    public async Task WhenGetAllFiles_ShouldReturnAllFiles()
    {
        var files = _fixture.CreateMany<BlobEntity>(5).ToList();
        foreach (var file in files)
        {
            await _repository.UpsertFileAsync(file);
        }

        var result = await _repository.GetAllFilesAsync();

        result.Should().BeEquivalentTo(files);
    }
}