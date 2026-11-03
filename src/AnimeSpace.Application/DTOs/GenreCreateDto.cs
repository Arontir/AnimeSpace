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
        [Required(ErrorMessage = "Вкажіть назву жанру.")]
        [StringLength(100, ErrorMessage = "Назва жанру має бути не довшою за 100 символів.")]
        public string Name { get; set; } = string.Empty;
    }
}
