using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Доступ до тайтлів (EF Core). Лише читає й записує дані: бізнес-логіки тут немає.
    /// Для показу дані читаються без відстеження (AsNoTracking), для редагування — з відстеженням.
    /// </summary>
    /// <param name="db">Контекст бази даних.</param>
    public class AnimeRepository(ApplicationDbContext db) : IAnimeRepository
    {
        /// <inheritdoc/>
        public async Task<Anime?> GetByIdAsync(int id)
            => await db.Animes.FirstOrDefaultAsync(a => a.Id == id);

        /// <inheritdoc/>
        public async Task<Anime?> GetForUpdateAsync(int id)
        {
            return await db.Animes
                .Include(a => a.Genres)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Anime?> GetDetailsAsync(int id)
        {
            return await db.Animes
                .AsNoTracking()
                .Include(a => a.Genres)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Anime>> GetFilteredAsync(AnimeFilterDto filter)
        {
            IQueryable<Anime> query = db.Animes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var pattern = $"%{EscapeLike(filter.SearchQuery)}%";
                query = query.Where(a => EF.Functions.Like(a.Title, pattern, "\\"));
            }

            if (filter.GenreId is int genreId)
            {
                query = query.Where(a => a.Genres.Any(g => g.Id == genreId));
            }

            if (filter.ReleaseYear is int year)
            {
                query = query.Where(a => a.ReleaseYear == year);
            }

            if (filter.Status is AnimeStatus status)
            {
                query = query.Where(a => a.Status == status);
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy switch
            {
                AnimeSortBy.TopRated => query
                    .OrderByDescending(a => a.AverageRating)
                    .ThenByDescending(a => a.RatingsCount)
                    .ThenBy(a => a.Title)
                    .ThenBy(a => a.Id),

                AnimeSortBy.TitleAsc => query
                    .OrderBy(a => a.Title)
                    .ThenBy(a => a.Id),

                AnimeSortBy.ReleaseYearDesc => query
                    .OrderByDescending(a => a.ReleaseYear)
                    .ThenBy(a => a.Title)
                    .ThenBy(a => a.Id),

                _ => query
                    .OrderByDescending(a => a.CreatedAt)
                    .ThenBy(a => a.Id)
            };

            var items = await query
                .Include(a => a.Genres)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Anime>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Anime>> GetLatestAsync(int count)
        {
            return await db.Animes
                .AsNoTracking()
                .Include(a => a.Genres)
                .OrderByDescending(a => a.CreatedAt)
                .ThenBy(a => a.Id)
                .Take(count)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Anime>> GetTopRatedAsync(int count)
        {
            return await db.Animes
                .AsNoTracking()
                .Include(a => a.Genres)
                .OrderByDescending(a => a.AverageRating)
                .ThenByDescending(a => a.RatingsCount)
                .ThenBy(a => a.Title)
                .ThenBy(a => a.Id)
                .Take(count)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int id)
            => await db.Animes.AnyAsync(a => a.Id == id);

        /// <inheritdoc/>
        public async Task AddAsync(Anime anime)
        {
            await db.Animes.AddAsync(anime);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Anime anime)
        {
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Anime anime)
        {
            db.Animes.Remove(anime);
            await db.SaveChangesAsync();
        }


        /// <summary>Екранує спецсимволи шаблону LIKE (\ % _ [), щоб вони сприймалися як звичайні символи.</summary>
        /// <param name="value">Рядок пошуку від користувача.</param>
        private static string EscapeLike(string value)
            => value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[");
    }
}
