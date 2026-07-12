namespace SamorodinkaTech.Fiducia.Infrastructure.FileStorage
{
    /// <summary>
    /// Опции файлового хранилища. Пока поддерживается только Local.
    /// Значения приходят из конфигурации (appsettings.json / env).
    /// </summary>
    public sealed class FileStorageOptions
    {
        /// <summary>
        /// Провайдер: Local | S3 (в будущем). Сейчас используется только Local.
        /// </summary>
        public string Provider { get; set; } = "Local";

        /// <summary>
        /// Базовый путь каталога для локального хранилища. Может быть относительным.
        /// </summary>
        public string? LocalBasePath { get; set; }
    }
}
