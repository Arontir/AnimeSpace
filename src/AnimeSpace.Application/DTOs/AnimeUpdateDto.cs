namespace AnimeSpace.Application.DTOs
{
    public class AnimeUpdateDto : AnimeCreateDto
    {
        public int Id { get; set; }
        public string? CurrentPosterUrl { get; set; }
    }
}
