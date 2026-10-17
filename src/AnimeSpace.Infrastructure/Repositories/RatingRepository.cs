using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Доступ до оцінок (EF Core). Підсумки рейтингу тайтла перераховуються одним запитом UPDATE у базі,
    /// тому одночасні оцінки не псують середнє.
    /// </summary>
    /// <param name="db">Контекст бази даних.</param>
    public class RatingRepository(ApplicationDbContext db) : IRatingRepository
    {
        /// <inheritdoc/>
        public async Task<Rating?> GetAsync(string userId, int animeId)
        {
            return await db.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.AnimeId == animeId);
        }

        /// <inheritdoc/>
        public async Task<RatingResultDto> RecalculateAverageRatingAsync(int animeId)
        {
            await db.Animes
                .Where(a => a.Id == animeId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.RatingsCount,
                        a => db.Ratings.Count(r => r.AnimeId == a.Id))
                    .SetProperty(a => a.AverageRating,
                        a => db.Ratings.Where(r => r.AnimeId == a.Id)
                        .Average(r => (double?)r.Score) ?? 0));

            return await db.Animes
                 .AsNoTracking()
                 .Where(a => a.Id == animeId)
                 .Select(a => new RatingResultDto
                 {
                     AverageRating = a.AverageRating,
                     RatingsCount = a.RatingsCount
                 })
                 .FirstAsync();
        }

        /// <inheritdoc/>
        public async Task AddAsync(Rating rating)
        {
            await db.Ratings.AddAsync(rating);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Rating rating)
        {
            await db.SaveChangesAsync();
        }
    }
}
