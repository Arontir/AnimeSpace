using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    /// <summary>
    /// Доступ до коментарів.
    /// </summary>
    public interface ICommentRepository
    {
        /// <summary>
        /// Повертає коментарі тайтлу разом з авторами (Include User), від новіших до старіших.
        /// Не більше 100 останніх. Без відстеження.
        /// </summary>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        public Task<IReadOnlyList<Comment>> GetByAnimeIdAsync(int animeId);

        /// <summary>
        /// Знаходить коментар разом з автором (Include User). Потрібен і для перевірки власника,
        /// і для повторного читання після додавання (щоб у DTO було ім'я автора).
        /// </summary>
        /// <param name="id">Ідентифікатор коментаря.</param>
        /// <returns>Коментар або <c>null</c>, якщо його не знайдено.</returns>
        public Task<Comment?> GetByIdAsync(int id);

        /// <summary>
        /// Додає коментар і зберігає зміни. Після виклику <c>comment.Id</c> заповнений.
        /// </summary>
        /// <param name="comment">Новий коментар.</param>
        public Task AddAsync(Comment comment);

        /// <summary>
        /// Видаляє коментар.
        /// </summary>
        /// <param name="comment">Коментар, завантажений раніше.</param>
        public Task DeleteAsync(Comment comment);
    }
}
