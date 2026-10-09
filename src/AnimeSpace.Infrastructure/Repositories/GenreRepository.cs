using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Доступ до жанрів (EF Core). Жанри з GetByIdAsync і GetByIdsAsync відстежуються EF навмисно:
    /// інакше при збереженні тайтла EF спробував би вставити їх у базу заново.
    /// </summary>
    /// <param name="db">Контекст бази даних.</param>
    public class GenreRepository(ApplicationDbContext db) : IGenreRepository
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<Genre>> GetAllAsync()
        {
            return await db.Genres
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Genre?> GetByIdAsync(int id)
            => await db.Genres.FirstOrDefaultAsync(g => g.Id == id);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var list = ids.Distinct().ToList();

            return await db.Genres
                .Where(g => list.Contains(g.Id))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> NameExistsAsync(string name, int? excludeGenreId = null)
            => await db.Genres.AnyAsync(g => g.Name == name && (excludeGenreId == null || g.Id != excludeGenreId));

        /// <inheritdoc/>
        public async Task AddAsync(Genre genre)
        {
            await db.Genres.AddAsync(genre);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Genre genre)
        {
            db.Genres.Update(genre);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Genre genre)
        {
            db.Genres.Remove(genre);
            await db.SaveChangesAsync();
        }

    }
}
