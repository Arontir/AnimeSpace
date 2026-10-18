using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Services;
using Moq;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Юніт-тест StatsService: сервіс лише передає результат репозиторію.
    /// </summary>
    public class StatsServiceTests
    {
        [Fact]
        public async Task GetDashboardStats_ReturnsRepositoryResult()
        {
            var stats = new DashboardStatsDto { UsersCount = 1, AnimeCount = 2, GenresCount = 3, CommentsCount = 4 };
            var repository = new Mock<IStatsRepository>();
            repository.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync(stats);

            var result = await new StatsService(repository.Object).GetDashboardStatsAsync();

            Assert.Same(stats, result);
        }
    }
}
