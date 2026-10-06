using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    public interface ICommentRepository
    {
        public Task<IReadOnlyList<Comment>> GetByAnimeIdAsync(int animeId);

        public Task<Comment?> GetByIdAsync(int id);

        public Task AddAsync(Comment comment);

        public Task DeleteAsync(Comment comment);
    }
}
