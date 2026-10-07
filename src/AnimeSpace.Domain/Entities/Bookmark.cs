using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Domain.Entities
{
    /// <summary>
    /// Закладка: тайтл в особистому списку користувача з певним статусом.
    /// Таблиця-зв'язка «користувачі ↔ аніме». Для пари (користувач, тайтл) існує не більше однієї закладки.
    /// </summary>
    public class Bookmark
    {
        /// <summary>
        /// Унікальний ідентифікатор закладки.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Статус тайтлу в списку користувача.
        /// </summary>
        public WatchStatus Status { get; set; } = WatchStatus.Planned;

        /// <summary>
        /// Дата останньої зміни статусу (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        /// <summary>
        /// Ідентифікатор користувача-власника закладки.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Користувач-власник. Заповнюється лише через Include.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;


        /// <summary>
        /// Ідентифікатор тайтлу.
        /// </summary>
        public int AnimeId { get; set; }

        /// <summary>
        /// Тайтл. Заповнюється лише через Include.
        /// </summary>
        public Anime Anime { get; set; } = null!;
    }
}