using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    public interface ICommentService
    {
        public Task<IReadOnlyList<CommentDto>> GetByAnimeIdAsync(int animeId);

        public Task<CommentDto> AddAsync(string userId, int animeId, string text);

        public Task DeleteAsync(int commentId, string userId, bool isAdmin);
    }
}
