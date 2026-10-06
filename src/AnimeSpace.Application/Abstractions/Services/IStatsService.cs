using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    public interface IStatsService
    {
        public Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
