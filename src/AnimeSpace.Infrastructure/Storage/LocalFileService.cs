using AnimeSpace.Application.Abstractions.Storage;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnimeSpace.Infrastructure.Storage
{
    /// <summary>
    /// Зберігає постери на диску, у папці wwwroot/uploads/posters.
    /// Папка й ім'я файлу вибираються тут: ім'я завжди нове (GUID), тож користувач не може вплинути на шлях запису.
    /// </summary>
    /// <param name="options">Налаштування сховища.</param>
    /// <param name="logger">Журнал: у нього потрапляють невдалі спроби видалення файлів.</param>
    public class LocalFileService(IOptions<FileStorageOptions> options, ILogger<LocalFileService> logger) : IFileService
    {
        /// <summary>
        /// Початок адреси будь-якого локального постера.
        /// </summary>
        private const string UrlPrefix = "/uploads/posters/";

        private readonly FileStorageOptions _options = EnsureConfigured(options.Value);

        /// <summary>
        /// Фізична папка для постерів.
        /// </summary>
        private string PostersDirectory => Path.Combine(_options.WebRootPath, "uploads", "posters");

        /// <inheritdoc/>
        public async Task<string> SavePosterAsync(FileUploadDto file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_options.AllowedExtensions.Contains(extension))
            {
                throw new ValidationException($"Дозволені формати: {string.Join(", ", _options.AllowedExtensions)}.");
            }

            if (!file.Content.CanSeek)
            {
                throw new ValidationException("Не вдалося визначити розмір файлу.");
            }

            if (file.Content.Length == 0)
            {
                throw new ValidationException("Файл порожній.");
            }

            if (file.Content.Length > _options.MaxFileSizeBytes)
            {
                throw new ValidationException(
                    $"Розмір файлу перевищує ліміт у {_options.MaxFileSizeBytes / (1024 * 1024)} МБ.");
            }

            file.Content.Position = 0;

            Directory.CreateDirectory(PostersDirectory);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(PostersDirectory, fileName);

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await file.Content.CopyToAsync(stream);
            }
            catch
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw;
            }

            return $"{UrlPrefix}{fileName}";
        }

        /// <inheritdoc/>
        public Task DeletePosterAsync(string? posterUrl)
        {
            if (string.IsNullOrWhiteSpace(posterUrl)
                || !posterUrl.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var fileName = Path.GetFileName(posterUrl);

            if (string.IsNullOrEmpty(fileName))
            {
                return Task.CompletedTask;
            }

            var fullPath = Path.Combine(PostersDirectory, fileName);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                logger.LogWarning(ex, "Не вдалося видалити файл постера {FilePath}.", fullPath);
            }

            return Task.CompletedTask;
        }

        /// <summary>Перевіряє, що шлях до wwwroot заданий).</summary>
        /// <param name="value">Налаштування сховища.</param>
        /// <exception cref="InvalidOperationException">Шлях до wwwroot не задано.</exception>
        private static FileStorageOptions EnsureConfigured(FileStorageOptions value)
        {
            if (string.IsNullOrWhiteSpace(value.WebRootPath))
            {
                throw new InvalidOperationException("FileStorage:WebRootPath не задано. Передайте шлях до wwwroot у AddInfrastructure.");
            }

            return value;
        }
    }
}