using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    /// <summary>
    /// Доступ до жанрів.
    /// </summary>
    public interface IGenreRepository
    {
        /// <summary>
        /// Повертає всі жанри за назвою (за алфавітом). Без відстеження.
        /// </summary>
        public Task<IReadOnlyList<Genre>> GetAllAsync();

        /// <summary>
        /// Знаходить жанр. Сутність відстежується EF (без AsNoTracking):
        /// інакше при збереженні тайтла EF спробує вставити жанр у базу заново.
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        /// <returns>Жанр або <c>null</c>, якщо його не знайдено.</returns>
        public Task<Genre?> GetByIdAsync(int id);

        /// <summary>
        /// Повертає жанри за списком ідентифікаторів. Сутності відстежуються EF
        /// (з тієї ж причини, що й у <see cref="GetByIdAsync"/>). Неіснуючі ідентифікатори просто не потрапляють у результат.
        /// </summary>
        /// <param name="ids">Ідентифікатори жанрів.</param>
        public Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<int> ids);

        /// <summary>Перевіряє, чи існує жанр із такою назвою.</summary>
        /// <param name="name">Назва для перевірки.</param>
        /// <param name="excludeGenreId">Ідентифікатор жанру, який треба виключити з перевірки (під час перейменування).</param>
        public Task<bool> NameExistsAsync(string name, int? excludeGenreId = null);

        /// <summary>
        /// Додає жанр і зберігає зміни.
        /// </summary>
        /// <param name="genre">Новий жанр.</param>
        public Task AddAsync(Genre genre);

        /// <summary>
        /// Зберігає зміни жанру, завантаженого через <see cref="GetByIdAsync"/>.
        /// </summary>
        /// <param name="genre">Змінений жанр.</param>
        public Task UpdateAsync(Genre genre);

        /// <summary>
        /// Видаляє жанр. Зв'язки з тайтлами видаляються каскадно.
        /// </summary>
        /// <param name="genre">Жанр, завантажений раніше.</param>
        public Task DeleteAsync(Genre genre);
    }
}
