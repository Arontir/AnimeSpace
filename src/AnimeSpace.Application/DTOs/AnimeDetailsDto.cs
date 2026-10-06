using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    public record AnimeDetailsDto
    {
        public required int Id { get; init; }
        public required string Title { get; init; }
        public required string? Description { get; init; }
        public required int ReleaseYear { get; init; }
        public required string? PosterUrl { get; init; }
        public required AnimeStatus Status { get; init; }
        public required double AverageRating { get; init; }
        public required int RatingsCount { get; init; }
        public required string? PlayerUrl { get; init; }
        public required List<GenreDto> Genres { get; init; }
    }
}
