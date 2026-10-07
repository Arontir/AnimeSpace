using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    /// <summary>
    /// Бізнес-логіка оцінювання тайтлів.
    /// </summary>
    public interface IRatingService
    {
        /// <summary>
        /// Зберігає оцінку користувача (нову або змінює наявну) і перераховує середню оцінку тайтлу.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <param name="score">Оцінка від 1 до 10.</param>
        /// <returns>Нова середня оцінка та кількість оцінок тайтлу.</returns>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Оцінка поза діапазоном від 1 до 10.</exception>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        public Task<RatingResultDto> SetRatingAsync(string userId, int animeId, int score);

        /// <summary>
        /// Повертає оцінку, яку користувач поставив тайтлу.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <returns>Оцінка або <c>null</c>, якщо користувач ще не оцінював тайтл.</returns>
        public Task<int?> GetUserRatingAsync(string userId, int animeId);
    }
}
