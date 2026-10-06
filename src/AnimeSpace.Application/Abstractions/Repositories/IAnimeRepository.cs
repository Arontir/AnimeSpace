using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    public interface IAnimeRepository
    {
        public Task<Anime?> GetByIdAsync(int id);

        public Task<Anime?> GetForUpdateAsync(int id);

        public Task<Anime?> GetDetailsAsync(int id);

        public Task<PagedResult<Anime>> GetFilteredAsync(AnimeFilterDto filter);

        public Task<IReadOnlyList<Anime>> GetLatestAsync(int count);

        public Task<IReadOnlyList<Anime>> GetTopRatedAsync(int count);

        public Task<bool> ExistsAsync(int id);

        public Task AddAsync(Anime anime);

        public Task UpdateAsync(Anime anime);

        public Task DeleteAsync(Anime anime);
    }
}
