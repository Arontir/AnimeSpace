using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    /// <summary>
    /// Статистика для панелі адміністратора.
    /// </summary>
    public interface IStatsService
    {
        /// <summary>
        /// Повертає кількість користувачів, тайтлів, жанрів і коментарів.
        /// </summary>
        public Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
