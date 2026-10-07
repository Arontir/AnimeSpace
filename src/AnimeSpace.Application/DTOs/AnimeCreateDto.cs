using AnimeSpace.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Дані форми створення тайтлу.
    /// </summary>
    public class AnimeCreateDto
    {
        /// <summary>
        /// Назва (обов'язкова, до 200 символів).
        /// </summary>
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Опис (до 4000 символів).
        /// </summary>
        [StringLength(4000)]
        public string? Description { get; set; }

        /// <summary>
        /// Рік виходу (1900–2100). За замовчуванням — поточний рік.
        /// </summary>
        [Range(1900, 2100)]
        public int ReleaseYear { get; set; } = DateTime.UtcNow.Year;

        /// <summary>
        /// Зовнішнє посилання на постер (лише http/https)
        /// </summary>
        [Url, StringLength(500)]
        public string? PosterUrl { get; set; }

        /// <summary>
        /// Посилання на плеєр. 
        /// </summary>
        [StringLength(1000)]
        public string? PlayerUrl { get; set; }

        /// <summary>
        /// Статус виходу. За замовчуванням — «Онгоїнг».
        /// </summary>
        public AnimeStatus Status { get; set; } = AnimeStatus.Ongoing;

        /// <summary>
        /// Ідентифікатори обраних жанрів. Порожній список допустимий.
        /// </summary>
        public List<int> SelectedGenreIds { get; set; } = new List<int>();
    }
}
