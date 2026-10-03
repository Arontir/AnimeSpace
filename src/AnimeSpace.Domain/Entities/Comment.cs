namespace AnimeSpace.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public required string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;


        public int AnimeId { get; set; }
        public Anime Anime { get; set; } = null!;
    }
}