using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Tests.Helpers;

namespace AnimeSpace.Tests.Integration
{
    /// <summary>
    /// Інтеграційний тест StatsRepository: підрахунок користувачів, тайтлів, жанрів і коментарів.
    /// </summary>
    public class StatsRepositoryTests : IDisposable
    {
        private readonly SqliteTestDb _db = new();

        public void Dispose() => _db.Dispose();

        [Fact]
        public async Task GetDashboardStats_CountsEverything()
        {
            var first = await _db.AddUserAsync("one");
            await _db.AddUserAsync("two");
            var anime = TestData.CreateAnime("A", 2020, AnimeSpace.Domain.Enums.AnimeStatus.Finished, TestData.CreateGenre("Action"));
            await _db.SeedAsync(anime, TestData.CreateAnime("B"), TestData.CreateAnime("C"));
            await _db.SeedAsync(new Comment { UserId = first.Id, AnimeId = anime.Id, Text = "x" });

            var stats = await new StatsRepository(_db.Context).GetDashboardStatsAsync();

            Assert.Equal(2, stats.UsersCount);
            Assert.Equal(3, stats.AnimeCount);
            Assert.Equal(1, stats.GenresCount);
            Assert.Equal(1, stats.CommentsCount);
        }
    }
}
