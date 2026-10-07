using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    /// <summary>
    /// Доступ до тайтлів. Репозиторій лише читає й записує дані, бізнес-логіки в ньому немає.
    /// Методи запису самі зберігають зміни в базі.
    /// </summary>
    public interface IAnimeRepository
    {
        /// <summary>
        /// Знаходить тайтл без пов'язаних даних. Сутність відстежується EF. Для перевірок і перерахунків.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        /// <returns>Тайтл або <c>null</c>, якщо його не знайдено.</returns>
        public Task<Anime?> GetByIdAsync(int id);

        /// <summary>
        /// Знаходить тайтл разом із жанрами (Include). Сутність відстежується EF. Для редагування.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        /// <returns>Тайтл із жанрами або <c>null</c>, якщо його не знайдено.</returns>
        public Task<Anime?> GetForUpdateAsync(int id);

        /// <summary>
        /// Знаходить тайтл разом із жанрами. Без відстеження (лише для читання). Для сторінки перегляду.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        /// <returns>Тайтл із жанрами або <c>null</c>, якщо його не знайдено.</returns>
        public Task<Anime?> GetDetailsAsync(int id);

        /// <summary>
        /// Повертає сторінку тайтлів за фільтром: пошук за назвою, жанр, рік, статус, сортування, пагінація.
        /// Без відстеження, із жанрами. Фільтр уже нормалізовано сервісом.
        /// </summary>
        /// <param name="filter">Нормалізований фільтр.</param>
        public Task<PagedResult<Anime>> GetFilteredAsync(AnimeFilterDto filter);

        /// <summary>
        /// Повертає останні додані тайтли (за датою додавання, від новіших) із жанрами.
        /// </summary>
        /// <param name="count">Кількість тайтлів.</param>
        public Task<IReadOnlyList<Anime>> GetLatestAsync(int count);

        /// <summary>
        /// Повертає тайтли з найвищим рейтингом: за середньою оцінкою (спадання), потім за кількістю оцінок (спадання), потім за назвою.
        /// Із жанрами.
        /// </summary>
        /// <param name="count">Кількість тайтлів.</param>
        public Task<IReadOnlyList<Anime>> GetTopRatedAsync(int count);

        /// <summary>
        /// Перевіряє, чи існує тайтл із таким ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        public Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Додає новий тайтл і зберігає зміни. Після виклику <c>anime.Id</c> заповнений.
        /// </summary>
        /// <param name="anime">Новий тайтл (разом із жанрами, якщо вони задані).</param>
        public Task AddAsync(Anime anime);

        /// <summary>
        /// Зберігає зміни тайтлу, завантаженого через <see cref="GetForUpdateAsync"/>.
        /// </summary>
        /// <param name="anime">Змінений тайтл.</param>
        public Task UpdateAsync(Anime anime);

        /// <summary>
        /// Видаляє тайтл разом із пов'язаними оцінками, коментарями та закладками (каскадно).
        /// </summary>
        /// <param name="anime">Тайтл, завантажений раніше.</param>
        public Task DeleteAsync(Anime anime);
    }
}
