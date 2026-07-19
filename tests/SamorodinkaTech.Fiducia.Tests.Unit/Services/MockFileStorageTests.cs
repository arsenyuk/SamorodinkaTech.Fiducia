using FluentAssertions;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты MockFileStorage — in-memory реализация IFileStorage.
/// </summary>
public class MockFileStorageTests
{
    private readonly MockFileStorage _storage = new();

    /// <summary>
    /// SaveAsync должен сохранять файл и возвращать непустой ключ.
    /// </summary>
    [Fact]
    public async Task SaveAsync_ShouldReturnStorageKey()
    {
        var content = new MemoryStream("test content"u8.ToArray());

        var key = await _storage.SaveAsync(content, "test.txt", "text/plain");

        key.Should().NotBeNullOrWhiteSpace();
        key.Should().Contain("/");
        key.Should().EndWith(".txt");
        _storage.FileCount.Should().Be(1);
    }

    /// <summary>
    /// OpenReadAsync должен возвращать поток с сохранённым содержимым.
    /// </summary>
    [Fact]
    public async Task OpenReadAsync_ShouldReturnSavedContent()
    {
        var originalData = "hello world"u8.ToArray();
        var content = new MemoryStream(originalData);
        var key = await _storage.SaveAsync(content, "file.txt");

        var result = await _storage.OpenReadAsync(key);
        using var ms = new MemoryStream();
        await result.CopyToAsync(ms);
        var buffer = ms.ToArray();

        buffer.Should().Equal(originalData);
    }

    /// <summary>
    /// OpenReadAsync должен выбрасывать FileNotFoundException для несуществующего ключа.
    /// </summary>
    [Fact]
    public async Task OpenReadAsync_WhenKeyNotFound_ShouldThrowFileNotFoundException()
    {
        Func<Task> act = () => _storage.OpenReadAsync("nonexistent");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    /// <summary>
    /// DeleteAsync должен возвращать true для существующего файла.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenFileExists_ShouldReturnTrue()
    {
        var content = new MemoryStream("data"u8.ToArray());
        var key = await _storage.SaveAsync(content, "file.txt");

        var deleted = await _storage.DeleteAsync(key);

        deleted.Should().BeTrue();
        _storage.FileCount.Should().Be(0);
        _storage.Exists(key).Should().BeFalse();
    }

    /// <summary>
    /// DeleteAsync должен возвращать false для несуществующего файла.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenFileNotFound_ShouldReturnFalse()
    {
        var deleted = await _storage.DeleteAsync("nonexistent");

        deleted.Should().BeFalse();
    }

    /// <summary>
    /// При SimulateFailure все методы должны выбрасывать InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task SimulateFailure_ShouldThrowOnAllOperations()
    {
        _storage.SimulateFailure = true;
        var content = new MemoryStream("data"u8.ToArray());

        Func<Task> save = () => _storage.SaveAsync(content, "file.txt");
        await save.Should().ThrowAsync<InvalidOperationException>();

        Func<Task> read = () => _storage.OpenReadAsync("key");
        await read.Should().ThrowAsync<InvalidOperationException>();

        Func<Task> delete = () => _storage.DeleteAsync("key");
        await delete.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>
    /// StorageKeys должен возвращать все сохранённые ключи.
    /// </summary>
    [Fact]
    public async Task StorageKeys_ShouldReturnAllSavedKeys()
    {
        var content1 = new MemoryStream("data1"u8.ToArray());
        var content2 = new MemoryStream("data2"u8.ToArray());

        var key1 = await _storage.SaveAsync(content1, "file1.txt");
        var key2 = await _storage.SaveAsync(content2, "file2.pdf");

        _storage.StorageKeys.Should().Contain(key1);
        _storage.StorageKeys.Should().Contain(key2);
        _storage.FileCount.Should().Be(2);
    }
}
