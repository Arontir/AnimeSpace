namespace AnimeSpace.Application.DTOs
{
    public record GenreDto
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
    }
}
