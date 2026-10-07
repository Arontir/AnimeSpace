using System.ComponentModel.DataAnnotations;

namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Дані форми створення або перейменування жанру.
    /// </summary>
    public class GenreCreateDto
    {
        /// <summary>
        /// Назва жанру (обов'язкова, до 100 символів, унікальна).
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
