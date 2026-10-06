using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    public interface IStatsRepository
    {
        public Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
