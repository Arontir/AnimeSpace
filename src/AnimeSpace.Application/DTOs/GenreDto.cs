namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Жанр для відображення та вибору у формах.
    /// </summary>
    public record GenreDto
    {
        /// <summary>
        /// Ідентифікатор жанру.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Назва жанру.
        /// </summary>
        public required string Name { get; init; }
    }
}
