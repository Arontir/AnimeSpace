namespace AnimeSpace.Application.DTOs
{
    public record CommentDto
    {
        public required int Id { get; init; }
        public required string Text { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required string UserId { get; init; }
        public required string UserName { get; init; }
    }
}
