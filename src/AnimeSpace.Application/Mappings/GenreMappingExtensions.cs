using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    public static class GenreMappingExtensions
    {
        public static GenreDto ToDto(this Genre genre) => new GenreDto()
        {
            Id = genre.Id,
            Name = genre.Name
        };

        public static Genre ToEntity(this GenreCreateDto dto) => new Genre()
        {
            Name = dto.Name
        };
    }
}
