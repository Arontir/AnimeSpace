namespace AnimeSpace.Domain.Entities
{
    /// <summary>
    /// Оцінка тайтлу користувачем. Кожен користувач може оцінити тайтл лише один раз.
    /// </summary>
    public class Rating
    {
        /// <summary>
        /// Мінімально допустима оцінка.
        /// </summary>
        public const int MinScore = 1;

        /// <summary>
        /// Максимально допустима оцінка.
        /// </summary>
        public const int MaxScore = 10;


        /// <summary>
        /// Унікальний ідентифікатор оцінки.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Оцінка від <see cref="MinScore"/> до <see cref="MaxScore"/>.
        /// </summary>
        public int Score { get; set; }


        /// <summary>
        /// Ідентифікатор користувача, який поставив оцінку.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Користувач. Заповнюється лише через Include.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;


        /// <summary>
        /// Ідентифікатор оціненого тайтлу.
        /// </summary>
        public int AnimeId { get; set; }

        /// <summary>
        /// Тайтл. Заповнюється лише через Include.
        /// </summary>
        public Anime Anime { get; set; } = null!;
    }
}