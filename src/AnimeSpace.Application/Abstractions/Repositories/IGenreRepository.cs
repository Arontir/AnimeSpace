using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    public interface IGenreRepository
    {
        public Task<IReadOnlyList<Genre>> GetAllAsync();

        public Task<Genre?> GetByIdAsync(int id);

        public Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<int> ids);

        public Task<bool> NameExistsAsync(string name, int? excludeGenreId = null);

        public Task AddAsync(Genre genre);

        public Task UpdateAsync(Genre genre);

        public Task DeleteAsync(Genre genre);
    }
}
