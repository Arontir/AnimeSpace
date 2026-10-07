using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    /// <summary>
    /// Перетворення між жанром та його DTO.
    /// </summary>
    public static class GenreMappingExtensions
    {
        /// <summary>
        /// Створює DTO жанру.
        /// </summary>
        /// <param name="genre">Жанр.</param>
        public static GenreDto ToDto(this Genre genre) => new GenreDto()
        {
            Id = genre.Id,
            Name = genre.Name
        };

        /// <summary>
        /// Створює нову сутність жанру з даних форми.
        /// </summary>
        /// <param name="dto">Дані форми.</param>
        public static Genre ToEntity(this GenreCreateDto dto) => new Genre()
        {
            Name = dto.Name
        };
    }
}
