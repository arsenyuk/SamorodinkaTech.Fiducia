namespace SamorodinkaTech.Fiducia.Infrastructure.FileStorage
{
    /// <summary>
    /// Опции файлового хранилища. Поддерживаются Local и MinIO.
    /// Значения приходят из конфигурации (appsettings.json / env).
    /// </summary>
    public sealed class FileStorageOptions
    {
        /// <summary>
        /// Провайдер: Local | MinIO.
        /// </summary>
        public string Provider { get; set; } = "Local";

        /// <summary>
        /// Базовый путь каталога для локального хранилища. Может быть относительным.
        /// </summary>
        public string? LocalBasePath { get; set; }

        /// <summary>
        /// URL MinIO-сервера (например, localhost:9000).
        /// </summary>
        public string? MinioEndpoint { get; set; }

        /// <summary>
        /// Ключ доступа MinIO (access key).
        /// </summary>
        public string? MinioAccessKey { get; set; }

        /// <summary>
        /// Секретный ключ MinIO (secret key).
        /// </summary>
        public string? MinioSecretKey { get; set; }

        /// <summary>
        /// Имя корзины MinIO.
        /// </summary>
        public string? MinioBucketName { get; set; }

        /// <summary>
        /// Использовать SSL для подключения к MinIO.
        /// </summary>
        public bool MinioUseSsl { get; set; }
    }
}
