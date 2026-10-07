using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Коротка інформація про тайтл для картки в каталозі та на головній.
    /// </summary>
    public record AnimeCardDto
    {
        /// <summary>
        /// Ідентифікатор тайтлу.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Назва.
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Адреса постера; <c>null</c> — показується заглушка.
        /// </summary>
        public required string? PosterUrl { get; init; }

        /// <summary>
        /// Рік виходу.
        /// </summary>
        public required int ReleaseYear { get; init; }

        /// <summary>
        /// Статус виходу.
        /// </summary>
        public required AnimeStatus Status { get; init; }

        /// <summary>
        /// Середня оцінка (0, якщо оцінок немає).
        /// </summary>
        public required double AverageRating { get; init; }

        /// <summary>
        /// Кількість оцінок.
        /// </summary>
        public required int RatingsCount { get; init; }

        /// <summary>
        /// Назви жанрів, відсортовані за алфавітом.
        /// </summary>
        public required List<string> GenreNames { get; init; }
    }
}
