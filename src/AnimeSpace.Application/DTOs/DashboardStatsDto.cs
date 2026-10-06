namespace AnimeSpace.Application.DTOs
{
    public record DashboardStatsDto
    {
        public required int UsersCount { get; init; }
        public required int AnimeCount { get; init; }
        public required int GenresCount { get; init; }
        public required int CommentsCount { get; init; }
    }
}
