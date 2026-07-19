using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.FileStorage
{
    /// <summary>
    /// Файловое хранилище на базе MinIO (S3-совместимое объектное хранилище).
    /// </summary>
    public sealed class MinioFileStorage : IFileStorage
    {
        private readonly ILogger<MinioFileStorage> _logger;
        private readonly IMinioClient _client;
        private readonly string _bucketName;

        public MinioFileStorage(IOptions<FileStorageOptions> options, ILogger<MinioFileStorage> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var opt = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(opt.MinioEndpoint))
                throw new ArgumentException("MinioEndpoint is required for MinIO provider");
            if (string.IsNullOrWhiteSpace(opt.MinioAccessKey))
                throw new ArgumentException("MinioAccessKey is required for MinIO provider");
            if (string.IsNullOrWhiteSpace(opt.MinioSecretKey))
                throw new ArgumentException("MinioSecretKey is required for MinIO provider");
            _bucketName = opt.MinioBucketName ?? "fiducia";

            _client = new MinioClient()
                .WithEndpoint(opt.MinioEndpoint)
                .WithCredentials(opt.MinioAccessKey, opt.MinioSecretKey)
                .WithSSL(opt.MinioUseSsl)
                .Build();

            // Бакет создаётся лениво при первом сохранении
        }

        public async Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
        {
            await EnsureBucketExistsAsync(cancellationToken);

            var ext = Path.GetExtension(fileName);
            var storageKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{ext}";

            var putArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(storageKey)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType ?? "application/octet-stream");

            await _client.PutObjectAsync(putArgs, cancellationToken);

            _logger.LogInformation("Saved file to MinIO: bucket={Bucket}, key={Key}", _bucketName, storageKey);
            return storageKey;
        }

        public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            var memoryStream = new MemoryStream();

            var getArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(storageKey)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                });

            await _client.GetObjectAsync(getArgs, cancellationToken);

            return memoryStream;
        }

        public async Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var removeArgs = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(storageKey);

                await _client.RemoveObjectAsync(removeArgs, cancellationToken);
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
        }

        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
            var exists = await _client.BucketExistsAsync(bucketExistsArgs, cancellationToken);
            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _client.MakeBucketAsync(makeBucketArgs, cancellationToken);
                _logger.LogInformation("Created MinIO bucket: {Bucket}", _bucketName);
            }
        }
    }
}
