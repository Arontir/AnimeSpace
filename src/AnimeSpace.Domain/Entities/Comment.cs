namespace AnimeSpace.Domain.Entities
{
    /// <summary>
    /// Коментар користувача до тайтлу.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Унікальний ідентифікатор коментаря.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текст коментаря (до 2000 символів). Зберігається як ввів користувач, екранується під час виведення.
        /// </summary>
        public required string Text { get; set; }

        /// <summary>
        /// Дата створення (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        /// <summary>
        /// Ідентифікатор автора.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Автор коментаря. Заповнюється лише через Include.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;


        /// <summary>
        /// Ідентифікатор тайтлу, до якого належить коментар.
        /// </summary>
        public int AnimeId { get; set; }

        /// <summary>
        /// Тайтл. Заповнюється лише через Include.
        /// </summary>
        public Anime Anime { get; set; } = null!;
    }
}