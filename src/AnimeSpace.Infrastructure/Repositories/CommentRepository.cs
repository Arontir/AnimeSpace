using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Доступ до коментарів (EF Core). Коментарі читаються разом з авторами (Include User).
    /// </summary>
    /// <param name="db">Контекст бази даних.</param>
    public class CommentRepository(ApplicationDbContext db) : ICommentRepository
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<Comment>> GetByAnimeIdAsync(int animeId)
        {
            int maxComments = 100;

            return await db.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.AnimeId == animeId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(maxComments)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Comment comment)
        {
            await db.Comments.AddAsync(comment);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Comment comment)
        {
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
        }
    }
}
