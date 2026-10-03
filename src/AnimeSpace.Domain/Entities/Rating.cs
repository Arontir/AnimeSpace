namespace AnimeSpace.Domain.Entities
{
    public class Rating
    {
        public const int MinScore = 1,
                         MaxScore = 10;

        public int Id { get; set; }
        public int Score { get; set; }


        public required string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;


        public int AnimeId { get; set; }
        public Anime Anime { get; set; } = null!;
    }
}