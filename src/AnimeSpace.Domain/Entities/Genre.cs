namespace AnimeSpace.Domain.Entities
{
    /// <summary>
    /// Жанр аніме (наприклад, «Екшн», «Комедія»). Назва жанру унікальна.
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Унікальний ідентифікатор жанру.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Назва жанру.
        /// </summary>
        public required string Name { get; set; }


        /// <summary>
        /// Тайтли цього жанру (зв'язок «багато-до-багатьох»).
        /// </summary>
        public ICollection<Anime> Animes { get; set; } = new List<Anime>();
    }
}