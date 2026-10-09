using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Repositories
{
    /// <summary>
    /// Статистика сайту для панелі адміністратора (EF Core).
    /// </summary>
    /// <param name="db">Контекст бази даних.</param>
    public class StatsRepository(ApplicationDbContext db) : IStatsRepository
    {
        /// <inheritdoc/>
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var usersCount = await db.Users.CountAsync();
            var animeCount = await db.Animes.CountAsync();
            var genresCount = await db.Genres.CountAsync();
            var commentsCount = await db.Comments.CountAsync();

            return new DashboardStatsDto
            {
                UsersCount = usersCount,
                AnimeCount = animeCount,
                GenresCount = genresCount,
                CommentsCount = commentsCount
            };
        }
    }
}
