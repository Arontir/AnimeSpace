namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Результат оцінювання: нові підсумки тайтлу, щоб оновити сторінку без перезавантаження.
    /// </summary>
    public record RatingResultDto
    {
        /// <summary>
        /// Нова середня оцінка тайтлу.
        /// </summary>
        public required double AverageRating { get; init; }

        /// <summary>
        /// Нова кількість оцінок тайтлу.
        /// </summary>
        public required int RatingsCount { get; init; }
    }
}
