using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация IFileStorage для unit-тестирования.
/// Хранит файлы в оперативной памяти.
/// </summary>
public class MockFileStorage : IFileStorage
{
    private readonly Dictionary<string, byte[]> _files = new();
    private readonly Dictionary<string, string> _contentTypes = new();

    /// <summary>Если true, все методы выбрасывают исключение (имитация сбоя).</summary>
    public bool SimulateFailure { get; set; }

    /// <inheritdoc />
    public Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var ext = Path.GetExtension(fileName);
        var storageKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{ext}";

        using var ms = new MemoryStream();
        content.CopyTo(ms);
        _files[storageKey] = ms.ToArray();

        if (contentType != null)
            _contentTypes[storageKey] = contentType;

        return Task.FromResult(storageKey);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        if (!_files.TryGetValue(storageKey, out var data))
            throw new FileNotFoundException($"File not found: {storageKey}");

        return Task.FromResult<Stream>(new MemoryStream(data));
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var removed = _files.Remove(storageKey);
        _contentTypes.Remove(storageKey);
        return Task.FromResult(removed);
    }

    /// <summary>
    /// Возвращает количество сохранённых файлов.
    /// </summary>
    public int FileCount => _files.Count;

    /// <summary>
    /// Возвращает сохранённые ключи файлов.
    /// </summary>
    public IReadOnlyCollection<string> StorageKeys => _files.Keys.ToArray();

    /// <summary>
    /// Проверяет существование файла по ключу.
    /// </summary>
    public bool Exists(string storageKey) => _files.ContainsKey(storageKey);

    private void ThrowIfFailure()
    {
        if (SimulateFailure)
            throw new InvalidOperationException("Simulated file storage failure");
    }
}
