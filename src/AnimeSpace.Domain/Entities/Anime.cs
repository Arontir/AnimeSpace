using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Domain.Entities
{
    public class Anime
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        public int ReleaseYear { get; set; }
        public string? PosterUrl { get; set; }
        public string? PlayerUrl { get; set; }

        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }

        public AnimeStatus Status { get; set; } = AnimeStatus.Ongoing;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }
}
