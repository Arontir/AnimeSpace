using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    public interface IGenreService
    {
        public Task<IReadOnlyList<GenreDto>> GetAllAsync();

        public Task<GenreDto> GetByIdAsync(int id);

        public Task<int> CreateAsync(GenreCreateDto dto);

        public Task UpdateAsync(int id, GenreCreateDto dto);

        public Task DeleteAsync(int id);
    }
}
