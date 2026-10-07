using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    /// <summary>
    /// Доступ до закладок користувачів. Рішення «створити чи оновити» приймає сервіс, а не репозиторій.
    /// </summary>
    public interface IBookmarkRepository
    {
        /// <summary>
        /// Знаходить закладку користувача на тайтл. Сутність відстежується EF.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <returns>Закладка або <c>null</c>, якщо користувач ще не додавав цей тайтл.</returns>
        public Task<Bookmark?> GetAsync(string userId, int animeId);

        /// <summary>
        /// Повертає всі закладки користувача разом із тайтлами (Include Anime). Без відстеження.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        public Task<IReadOnlyList<Bookmark>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Додає нову закладку й зберігає зміни. Для пари (користувач, тайтл) діє унікальний індекс.
        /// </summary>
        /// <param name="bookmark">Нова закладка.</param>
        public Task AddAsync(Bookmark bookmark);

        /// <summary>
        /// Зберігає зміни закладки, завантаженої через <see cref="GetAsync"/>.
        /// </summary>
        /// <param name="bookmark">Змінена закладка.</param>
        public Task UpdateAsync(Bookmark bookmark);

        /// <summary>
        /// Видаляє закладку.
        /// </summary>
        /// <param name="bookmark">Закладка, завантажена раніше.</param>
        public Task DeleteAsync(Bookmark bookmark);
    }
}
