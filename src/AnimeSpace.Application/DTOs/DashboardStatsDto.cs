namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Статистика сайту для панелі адміністратора.
    /// </summary>
    public record DashboardStatsDto
    {
        /// <summary>
        /// Кількість зареєстрованих користувачів.
        /// </summary>
        public required int UsersCount { get; init; }

        /// <summary>
        /// Кількість тайтлів.
        /// </summary>
        public required int AnimeCount { get; init; }

        /// <summary>
        /// Кількість жанрів.
        /// </summary>
        public required int GenresCount { get; init; }

        /// <summary>
        /// Кількість коментарів.
        /// </summary>
        public required int CommentsCount { get; init; }
    }
}
