using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Domain.Entities
{
    public class Bookmark
    {
        public int Id { get; set; }
        public WatchStatus Status { get; set; } = WatchStatus.Planned;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public required string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;


        public int AnimeId { get; set; }
        public Anime Anime { get; set; } = null!;
    }
}