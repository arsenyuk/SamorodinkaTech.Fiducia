using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.FileStorage
{
    /// <summary>
    /// Регистрация файлового хранилища в DI. Выбирает реализацию по значению FileStorage:Provider.
    /// </summary>
    public static class FileStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет файловое хранилище: Local или MinIO.
        /// </summary>
        /// <param name="services">Коллекция сервисов DI.</param>
        /// <param name="configuration">Конфигурация приложения (секция FileStorage).</param>
        public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));

            var provider = configuration["FileStorage:Provider"] ?? "Local";

            if (provider == "MinIO")
            {
                services.AddSingleton<IFileStorage, MinioFileStorage>();
            }
            else
            {
                services.AddSingleton<IFileStorage, LocalFileStorage>();
            }

            return services;
        }
    }
}
