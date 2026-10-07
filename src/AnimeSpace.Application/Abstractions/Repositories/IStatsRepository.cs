using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    /// <summary>
    /// Доступ до статистики сайту.
    /// </summary>
    public interface IStatsRepository
    {
        /// <summary>
        /// Підраховує користувачів, тайтли, жанри та коментарі.
        /// </summary>
        public Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
