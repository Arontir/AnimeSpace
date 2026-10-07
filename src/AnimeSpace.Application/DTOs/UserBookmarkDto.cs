using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Закладка для відображення в особистому кабінеті користувача.
    /// </summary>
    public record UserBookmarkDto
    {
        /// <summary>
        /// Ідентифікатор тайтлу.
        /// </summary>
        public required int AnimeId { get; init; }

        /// <summary>
        /// Назва тайтлу.
        /// </summary>
        public required string AnimeTitle { get; init; }

        /// <summary>
        /// Адреса постера; <c>null</c> — показується заглушка.
        /// </summary>
        public required string? AnimePosterUrl { get; init; }

        /// <summary>
        /// Статус тайтлу в списку користувача.
        /// </summary>
        public required WatchStatus Status { get; init; }

        /// <summary>
        /// Дата останньої зміни статусу (UTC).
        /// </summary>
        public required DateTime UpdatedAt { get; init; }
    }
}
