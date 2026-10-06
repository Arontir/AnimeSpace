using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.Abstractions.Services
{
    public interface IBookmarkService
    {
        public Task SetStatusAsync(string userId, int animeId, WatchStatus status);

        public Task<WatchStatus?> GetStatusAsync(string userId, int animeId);

        public Task<IReadOnlyList<UserBookmarkDto>> GetUserBookmarksAsync(string userId);

        public Task DeleteAsync(string userId, int animeId);
    }
}