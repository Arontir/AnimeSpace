using System.ComponentModel.DataAnnotations;

namespace AnimeSpace.Application.DTOs
{
    public class GenreCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
