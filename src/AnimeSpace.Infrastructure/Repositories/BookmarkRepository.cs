using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Доступ до закладок користувачів (EF Core). Лише читає й записує дані:
    /// рішення «створити чи оновити» приймає BookmarkService.
    /// </summary>
    /// <param name="db">Контекст бази даних.</param>
    public class BookmarkRepository(ApplicationDbContext db) : IBookmarkRepository
    {
        /// <inheritdoc/>
        public async Task<Bookmark?> GetAsync(string userId, int animeId)
            => await db.Bookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.AnimeId == animeId);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Bookmark>> GetByUserIdAsync(string userId)
        {
            return await db.Bookmarks
                .AsNoTracking()
                .Include(b => b.Anime)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AddAsync(Bookmark bookmark)
        {
            await db.Bookmarks.AddAsync(bookmark);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Bookmark bookmark)
        {
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Bookmark bookmark)
        {
            db.Bookmarks.Remove(bookmark);
            await db.SaveChangesAsync();
        }
    }
}
