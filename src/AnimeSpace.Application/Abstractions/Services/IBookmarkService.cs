using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.Abstractions.Services
{
    /// <summary>
    /// Бізнес-логіка особистих списків («Дивлюсь», «У планах» тощо).
    /// </summary>
    public interface IBookmarkService
    {
        /// <summary>
        /// Встановлює статус тайтлу в списку користувача: якщо закладка вже є — змінює статус і дату,
        /// якщо немає — створює нову (дублікатів не буває).
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <param name="status">Новий статус.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Невідомий статус.</exception>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        public Task SetStatusAsync(string userId, int animeId, WatchStatus status);

        /// <summary>
        /// Повертає статус тайтлу в списку користувача.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <returns>Статус або <c>null</c>, якщо тайтлу немає в списках користувача.</returns>
        public Task<WatchStatus?> GetStatusAsync(string userId, int animeId);

        /// <summary>
        /// Повертає всі закладки користувача, від нещодавно змінених.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        public Task<IReadOnlyList<UserBookmarkDto>> GetUserBookmarksAsync(string userId);

        /// <summary>
        /// Видаляє закладку користувача.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Закладку не знайдено.</exception>
        public Task DeleteAsync(string userId, int animeId);
    }
}