using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    public interface IBookmarkRepository
    {
        public Task<Bookmark?> GetAsync(string userId, int animeId);

        public Task<IReadOnlyList<Bookmark>> GetByUserIdAsync(string userId);

        public Task AddAsync(Bookmark bookmark);

        public Task UpdateAsync(Bookmark bookmark);

        public Task DeleteAsync(Bookmark bookmark);
    }
}
