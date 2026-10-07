using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Повна інформація про тайтл для сторінки перегляду.
    /// </summary>
    public record AnimeDetailsDto
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
        /// Опис; <c>null</c>, якщо його не заповнено.
        /// </summary>
        public required string? Description { get; init; }

        /// <summary>
        /// Рік виходу.
        /// </summary>
        public required int ReleaseYear { get; init; }

        /// <summary>
        /// Адреса постера; <c>null</c> — показується заглушка.
        /// </summary>
        public required string? PosterUrl { get; init; }

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
        /// Посилання на плеєр для вставки в iframe; <c>null</c> — відео ще не додано.
        /// </summary>
        public required string? PlayerUrl { get; init; }

        /// <summary>
        /// Жанри тайтлу, відсортовані за назвою.
        /// </summary>
        public required List<GenreDto> Genres { get; init; }
    }
}
