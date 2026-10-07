namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Коментар для відображення на сторінці тайтлу.
    /// </summary>
    public record CommentDto
    {
        /// <summary>
        /// Ідентифікатор коментаря.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Текст коментаря.
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// Дата створення (UTC).
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Ідентифікатор автора.
        /// </summary>
        public required string UserId { get; init; }

        /// <summary>
        /// Ім'я автора.
        /// </summary>
        public required string UserName { get; init; }
    }
}
