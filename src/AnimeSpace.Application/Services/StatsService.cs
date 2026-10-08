using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Services
{
    /// <summary>
    /// Статистика для панелі адміністратора.
    /// Додаткової логіки тут немає: сервіс існує заради єдиного правила «контролери не звертаються до репозиторіїв напряму».
    /// </summary>
    /// <param name="stats">Репозиторій статистики.</param>
    public class StatsService(IStatsRepository stats) : IStatsService
    {
        /// <inheritdoc/>
        public Task<DashboardStatsDto> GetDashboardStatsAsync() => stats.GetDashboardStatsAsync();
    }
}
