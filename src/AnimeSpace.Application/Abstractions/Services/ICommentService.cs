using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    /// <summary>
    /// Бізнес-логіка коментарів до тайтлів.
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// Повертає коментарі тайтлу (до 100 останніх, від новіших).
        /// </summary>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        public Task<IReadOnlyList<CommentDto>> GetByAnimeIdAsync(int animeId);

        /// <summary>
        /// Додає коментар: обрізає зайві пробіли, перевіряє, що текст не порожній і не довший за 2000 символів.
        /// Після збереження читає коментар із бази ще раз, щоб у результаті було ім'я автора.
        /// </summary>
        /// <param name="userId">Ідентифікатор автора.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <param name="text">Текст коментаря.</param>
        /// <returns>Створений коментар.</returns>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Текст порожній або задовгий.</exception>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        public Task<CommentDto> AddAsync(string userId, int animeId, string text);

        /// <summary>
        /// Видаляє коментар. Адміністратор може видалити будь-який, користувач — лише свій.
        /// </summary>
        /// <param name="commentId">Ідентифікатор коментаря.</param>
        /// <param name="userId">Ідентифікатор користувача, який видаляє.</param>
        /// <param name="isAdmin">Чи має цей користувач роль адміністратора.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Коментар не знайдено.</exception>
        /// <exception cref="AnimeSpace.Application.Exceptions.ForbiddenException">Чужий коментар і користувач не адміністратор.</exception>
        public Task DeleteAsync(int commentId, string userId, bool isAdmin);
    }
}
