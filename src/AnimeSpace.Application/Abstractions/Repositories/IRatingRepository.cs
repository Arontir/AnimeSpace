using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    /// <summary>
    /// Доступ до оцінок і підсумків рейтингу тайтлів.
    /// </summary>
    public interface IRatingRepository
    {
        /// <summary>Знаходить оцінку користувача для тайтла. Сутність відстежується EF.</summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <returns>Оцінка або <c>null</c>, якщо користувач ще не оцінював тайтл.</returns>
        public Task<Rating?> GetAsync(string userId, int animeId);

        /// <summary>
        /// Додає нову оцінку й зберігає зміни. Для пари (користувач, тайтл) діє унікальний індекс.
        /// </summary>
        /// <param name="rating">Нова оцінка.</param>
        public Task AddAsync(Rating rating);

        /// <summary>
        /// Зберігає зміни оцінки, завантаженої через <see cref="GetAsync"/>.
        /// </summary>
        /// <param name="rating">Змінена оцінка.</param>
        public Task UpdateAsync(Rating rating);

        /// <summary>
        /// Перераховує середню оцінку й кількість оцінок тайтлу одним запитом до бази (UPDATE із підзапитом),
        /// тому одночасні оцінки не псують середнє. Викликається після збереження оцінки.
        /// </summary>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        /// <returns>Нові підсумки: середня оцінка та кількість оцінок.</returns>
        public Task<RatingResultDto> RecalculateAverageRatingAsync(int animeId);
    }
}
