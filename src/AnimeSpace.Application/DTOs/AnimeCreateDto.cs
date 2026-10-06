using AnimeSpace.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnimeSpace.Application.DTOs
{
    public class AnimeCreateDto
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Range(1900, 2100)]
        public int ReleaseYear { get; set; } = DateTime.UtcNow.Year;

        [Url, StringLength(500)]
        public string? PosterUrl { get; set; }

        [StringLength(1000)]
        public string? PlayerUrl { get; set; }

        public AnimeStatus Status { get; set; } = AnimeStatus.Ongoing;
        public List<int> SelectedGenreIds { get; set; } = new List<int>();
    }
}
