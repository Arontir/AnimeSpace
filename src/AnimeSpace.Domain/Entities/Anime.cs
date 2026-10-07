using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Domain.Entities
{
    /// <summary>
    /// Аніме-тайтл: основна сутність каталогу.
    /// </summary>
    public class Anime
    {
        /// <summary>
        /// Унікальний ідентифікатор тайтлу.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Назва тайтлу.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Опис тайтлу. Може бути відсутнім.
        /// </summary>
        public string? Description { get; set; }


        /// <summary>
        /// Рік виходу.
        /// </summary>
        public int ReleaseYear { get; set; }

        /// <summary>
        /// Адреса постера: зовнішнє посилання (http/https) або локальний шлях на кшталт "/uploads/posters/x.jpg".
        /// <c>null</c> — постера немає.
        /// </summary>
        public string? PosterUrl { get; set; }

        /// <summary>
        /// Посилання на вбудований плеєр (iframe) для всього тайтлу: YouTube-плейлист, Kodik тощо.
        /// Серії та озвучки перемикає сам плеєр. 
        /// <c>null</c> — відео ще не додано.
        /// </summary>
        public string? PlayerUrl { get; set; }


        /// <summary>
        /// Середня оцінка (збережений підсумок для швидкого каталогу та сортування). 0 — оцінок ще немає.
        /// Джерело істини — таблиця оцінок; значення перераховується при кожній новій оцінці.
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Кількість оцінок (збережений підсумок, оновлюється разом із середньою оцінкою).
        /// </summary>
        public int RatingsCount { get; set; }


        /// <summary>
        /// Статус виходу тайтлу.
        /// </summary>
        public AnimeStatus Status { get; set; } = AnimeStatus.Ongoing;

        /// <summary>
        /// Дата додавання тайтлу на сайт (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        /// <summary>
        /// Жанри тайтлу (зв'язок «багато-до-багатьох»).
        /// Заповнюється лише якщо репозиторій завантажив їх через Include.
        /// </summary>
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }
}
