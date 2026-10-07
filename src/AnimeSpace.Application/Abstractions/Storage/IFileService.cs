using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Storage
{
    /// <summary>
    /// Збереження файлів постерів. Реалізація лежить в Infrastructure.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Зберігає постер: перевіряє розмір і розширення (.jpg, .jpeg, .png, .webp),
        /// записує у фіксовану папку під новим випадковим іменем. Папку та ім'я файлу вибирає сам сервіс.
        /// </summary>
        /// <param name="file">Завантажений файл.</param>
        /// <returns>Відносна адреса збереженого файлу, наприклад "/uploads/posters/{guid}.jpg".</returns>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Недозволене розширення або завеликий файл.</exception>
        public Task<string> SavePosterAsync(FileUploadDto file);

        /// <summary>
        /// Видаляє файл постера. Нічого не робить для <c>null</c> і зовнішніх посилань;
        /// видаляє лише файли всередині "/uploads/posters/".
        /// </summary>
        /// <param name="posterUrl">Адреса постера з тайтла.</param>
        public Task DeletePosterAsync(string? posterUrl);
    }
}