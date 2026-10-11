namespace AnimeSpace.Infrastructure.Storage
{
    /// <summary>
    /// Налаштування збереження файлів (секція "FileStorage" у appsettings.json).
    /// </summary>
    public class FileStorageOptions
    {
        /// <summary>
        /// Назва секції в конфігурації.
        /// </summary>
        public const string SectionName = "FileStorage";

        /// <summary>
        /// Абсолютний шлях до папки wwwroot. Заповнюється під час реєстрації сервісів (AddInfrastructure), а не з конфігурації.
        /// </summary>
        public string WebRootPath { get; set; } = string.Empty;

        /// <summary>
        /// Максимальний розмір файлу в байтах. За замовчуванням — 5 МБ.
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

        /// <summary>
        /// Дозволені розширення файлів (малими літерами, з крапкою).
        /// Список задається лише тут, а не в appsettings.json.
        /// </summary>
        public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    }
}