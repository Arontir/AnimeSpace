namespace AnimeSpace.Application.DTOs
{
    public record RatingResultDto
    {
        public required double AverageRating { get; init; }
        public required int RatingsCount { get; init; }
    }
}
