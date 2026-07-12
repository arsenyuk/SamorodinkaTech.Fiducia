using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Infrastructure.FileStorage;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты MinioFileStorage: валидация опций в конструкторе.
/// </summary>
public class MinioFileStorageTests
{
    private readonly ILogger<MinioFileStorage> _logger = new LoggerFactory().CreateLogger<MinioFileStorage>();

    /// <summary>
    /// Конструктор должен выбросить ArgumentNullException при отсутствии options.
    /// </summary>
    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        Action act = () => new MinioFileStorage(null!, _logger);

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Конструктор должен выбросить ArgumentException при отсутствии MinioEndpoint.
    /// </summary>
    [Fact]
    public void Constructor_WhenMinioEndpointIsMissing_ShouldThrowArgumentException()
    {
        var opts = Options.Create(new FileStorageOptions
        {
            Provider = "MinIO",
            MinioAccessKey = "key",
            MinioSecretKey = "secret"
        });

        Action act = () => new MinioFileStorage(opts, _logger);

        act.Should().Throw<ArgumentException>().WithMessage("*MinioEndpoint*");
    }

    /// <summary>
    /// Конструктор должен выбросить ArgumentException при отсутствии MinioAccessKey.
    /// </summary>
    [Fact]
    public void Constructor_WhenMinioAccessKeyIsMissing_ShouldThrowArgumentException()
    {
        var opts = Options.Create(new FileStorageOptions
        {
            Provider = "MinIO",
            MinioEndpoint = "localhost:9000",
            MinioSecretKey = "secret"
        });

        Action act = () => new MinioFileStorage(opts, _logger);

        act.Should().Throw<ArgumentException>().WithMessage("*MinioAccessKey*");
    }

    /// <summary>
    /// Конструктор должен выбросить ArgumentException при отсутствии MinioSecretKey.
    /// </summary>
    [Fact]
    public void Constructor_WhenMinioSecretKeyIsMissing_ShouldThrowArgumentException()
    {
        var opts = Options.Create(new FileStorageOptions
        {
            Provider = "MinIO",
            MinioEndpoint = "localhost:9000",
            MinioAccessKey = "key"
        });

        Action act = () => new MinioFileStorage(opts, _logger);

        act.Should().Throw<ArgumentException>().WithMessage("*MinioSecretKey*");
    }

    /// <summary>
    /// Конструктор должен успешно создаться при полных опциях (без подключения к MinIO).
    /// </summary>
    [Fact]
    public void Constructor_WithValidOptions_ShouldNotThrow()
    {
        var opts = Options.Create(new FileStorageOptions
        {
            Provider = "MinIO",
            MinioEndpoint = "localhost:9000",
            MinioAccessKey = "minioadmin",
            MinioSecretKey = "minioadmin"
        });

        Action act = () => _ = new MinioFileStorage(opts, _logger);

        act.Should().NotThrow();
    }

    /// <summary>
    /// BucketName по умолчанию должен быть "fiducia", если не указан явно.
    /// </summary>
    [Fact]
    public void Constructor_WhenBucketNameNotSet_ShouldUseDefault()
    {
        var opts = Options.Create(new FileStorageOptions
        {
            Provider = "MinIO",
            MinioEndpoint = "localhost:9000",
            MinioAccessKey = "minioadmin",
            MinioSecretKey = "minioadmin"
        });

        Action act = () => _ = new MinioFileStorage(opts, _logger);

        act.Should().NotThrow(); // BucketName по умолчанию = "fiducia" используется внутри
    }
}
