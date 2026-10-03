namespace AnimeSpace.Domain.Entities
{
    public class Genre
    {
        public int Id { get; set; }
        public required string Name { get; set; }


        public ICollection<Anime> Animes { get; set; } = new List<Anime>();
    }
}