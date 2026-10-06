using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    public record UserBookmarkDto
    {
        public required int AnimeId { get; init; }
        public required string AnimeTitle { get; init; }
        public required string? AnimePosterUrl { get; init; }
        public required WatchStatus Status { get; init; }
        public required DateTime UpdatedAt { get; init; }
    }
}
