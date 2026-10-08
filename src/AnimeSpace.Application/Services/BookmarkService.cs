using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Mappings;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.Services
{
    /// <summary>
    /// Бізнес-логіка особистих списків («Дивлюсь», «У планах» тощо).
    /// </summary>
    /// <param name="bookmarks">Репозиторій закладок.</param>
    /// <param name="animes">Репозиторій тайтлів (для перевірки, що тайтл існує).</param>
    public class BookmarkService(IBookmarkRepository bookmarks, IAnimeRepository animes) : IBookmarkService
    {
        /// <inheritdoc/>
        public async Task SetStatusAsync(string userId, int animeId, WatchStatus status)
        {
            if (!Enum.IsDefined(status))
            {
                throw new ValidationException("Невідомий статус.");
            }

            if (!await animes.ExistsAsync(animeId))
            {
                throw new NotFoundException("Аніме не знайдено.");
            }

            var bookmark = await bookmarks.GetAsync(userId, animeId);

            if (bookmark is null)
            {
                await bookmarks.AddAsync(new Bookmark
                {
                    UserId = userId,
                    AnimeId = animeId,
                    Status = status,
                    UpdatedAt = DateTime.UtcNow
                });

                return;
            }

            bookmark.Status = status;
            bookmark.UpdatedAt = DateTime.UtcNow;

            await bookmarks.UpdateAsync(bookmark);
        }

        /// <inheritdoc/>
        public async Task<WatchStatus?> GetStatusAsync(string userId, int animeId)
        {
            var bookmark = await bookmarks.GetAsync(userId, animeId);
            return bookmark?.Status;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserBookmarkDto>> GetUserBookmarksAsync(string userId)
        {
            var list = await bookmarks.GetByUserIdAsync(userId);

            return list
                .OrderByDescending(b => b.UpdatedAt)
                .Select(b => b.ToDto())
                .ToList();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string userId, int animeId)
        {
            var bookmark = await bookmarks.GetAsync(userId, animeId) ?? throw new NotFoundException("Закладку не знайдено.");

            await bookmarks.DeleteAsync(bookmark);
        }
    }
}