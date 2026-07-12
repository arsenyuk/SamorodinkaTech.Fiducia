using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces
{
    /// <summary>
    /// Абстракция файлового хранилища. Хранит бинарные объекты и возвращает ключ хранения.
    /// Реализация Local сохраняет файлы в каталог на локальной ФС.
    /// </summary>
    public interface IFileStorage
    {
        /// <summary>
        /// Сохраняет поток в хранилище и возвращает ключ хранения (storage key/path).
        /// </summary>
        /// <param name="content">Поток содержимого. Не закрывается реализацией.</param>
        /// <param name="fileName">Оригинальное имя файла (для расширения/логов).</param>
        /// <param name="contentType">MIME тип (опционально, может использоваться для валидации).</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Открывает поток чтения по ключу хранения.
        /// </summary>
        Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет файл по ключу хранения. Возвращает true, если файл был удалён.
        /// </summary>
        Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
