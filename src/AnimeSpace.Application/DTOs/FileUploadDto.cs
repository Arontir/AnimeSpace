namespace AnimeSpace.Application.DTOs
{
    public record FileUploadDto
    {
        public required Stream Content { get; init; }
        public required string FileName { get; init; }
    }
}
