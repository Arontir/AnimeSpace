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
        [Required(ErrorMessage = "Вкажіть назву аніме.")]
        [StringLength(200, ErrorMessage = "Назва має бути не довшою за 200 символів.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Опис (до 4000 символів).
        /// </summary>
        [StringLength(4000, ErrorMessage = "Опис має бути не довшим за 4000 символів.")]
        public string? Description { get; set; }

        /// <summary>
        /// Рік виходу (1900–2100). За замовчуванням — поточний рік.
        /// </summary>
        [Range(1900, 2100, ErrorMessage = "Рік виходу має бути від 1900 до 2100.")]
        public int ReleaseYear { get; set; } = DateTime.UtcNow.Year;

        /// <summary>
        /// Зовнішнє посилання на постер (лише http/https)
        /// </summary>
        [Url(ErrorMessage = "Вкажіть коректне посилання (https://).")]
        [StringLength(500, ErrorMessage = "Посилання має бути не довше за 500 символів.")]
        public string? PosterUrl { get; set; }

        /// <summary>
        /// Посилання на плеєр. Атрибут [Url] навмисно не застосовується: він відхилив би посилання виду "//kodik.info/...".
        /// Перевірку й нормалізацію виконує AnimeService.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Посилання має бути не довше за 1000 символів.")]
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
