using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Tests.Integration
{
    /// <summary>
    /// Інтеграційні тести RatingRepository. Головне: підрахунок середньої оцінки одним UPDATE у базі
    /// та обмеження бази (унікальність пари, діапазон оцінки).
    /// </summary>
    public class RatingRepositoryTests : IDisposable
    {
        private readonly SqliteTestDb _db = new();
        private readonly RatingRepository _repo;

        public RatingRepositoryTests()
        {
            _repo = new RatingRepository(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        /// <summary>
        /// Один тайтл і двоє користувачів.
        /// </summary>
        private async Task<(Anime Anime, ApplicationUser First, ApplicationUser Second)> ArrangeAsync()
        {
            var anime = TestData.CreateAnime("A");
            var first = await _db.AddUserAsync("one");
            var second = await _db.AddUserAsync("two");
            await _db.SeedAsync(anime);
            return (anime, first, second);
        }

        [Fact]
        public async Task Recalculate_TwoRatings_AverageIsExactlyNine()
        {
            var (anime, first, second) = await ArrangeAsync();
            await _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 10 });
            await _repo.AddAsync(new Rating { UserId = second.Id, AnimeId = anime.Id, Score = 8 });

            var result = await _repo.RecalculateAverageRatingAsync(anime.Id);

            Assert.Equal(9.0, result.AverageRating);
            Assert.Equal(2, result.RatingsCount);
        }

        [Fact]
        public async Task Recalculate_StoresTheResultInTheTitle()
        {
            var (anime, first, second) = await ArrangeAsync();
            await _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 10 });
            await _repo.AddAsync(new Rating { UserId = second.Id, AnimeId = anime.Id, Score = 7 });

            await _repo.RecalculateAverageRatingAsync(anime.Id);

            using var check = _db.CreateNewContext();
            var saved = await check.Animes.AsNoTracking().SingleAsync(a => a.Id == anime.Id);
            Assert.Equal(8.5, saved.AverageRating);
            Assert.Equal(2, saved.RatingsCount);
        }

        [Fact]
        public async Task Recalculate_NoRatings_GivesZeroes()
        {
            var (anime, _, _) = await ArrangeAsync();

            var result = await _repo.RecalculateAverageRatingAsync(anime.Id);

            Assert.Equal(0.0, result.AverageRating);
            Assert.Equal(0, result.RatingsCount);
        }

        [Fact]
        public async Task Recalculate_OnlyAffectsTheGivenTitle()
        {
            var (anime, first, _) = await ArrangeAsync();
            var other = TestData.CreateAnime("Other");
            await _db.SeedAsync(other);
            await _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 10 });

            await _repo.RecalculateAverageRatingAsync(anime.Id);

            using var check = _db.CreateNewContext();
            var untouched = await check.Animes.AsNoTracking().SingleAsync(a => a.Id == other.Id);
            Assert.Equal(0.0, untouched.AverageRating);
            Assert.Equal(0, untouched.RatingsCount);
        }

        [Fact]
        public async Task Update_ChangesScore_AndKeepsSingleRow()
        {
            var (anime, first, second) = await ArrangeAsync();
            await _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 10 });
            await _repo.AddAsync(new Rating { UserId = second.Id, AnimeId = anime.Id, Score = 8 });

            var rating = await _repo.GetAsync(first.Id, anime.Id);
            rating!.Score = 4;
            await _repo.UpdateAsync(rating);

            var result = await _repo.RecalculateAverageRatingAsync(anime.Id);

            using var check = _db.CreateNewContext();
            Assert.Equal(2, await check.Ratings.CountAsync());
            Assert.Equal(6.0, result.AverageRating);
        }

        [Fact]
        public async Task Add_SecondRatingFromSameUser_ViolatesUniqueIndex()
        {
            var (anime, first, _) = await ArrangeAsync();
            await _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 5 });

            await Assert.ThrowsAsync<DbUpdateException>(
                () => _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 6 }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public async Task Add_ScoreOutOfRange_ViolatesCheckConstraint(int score)
        {
            var (anime, first, _) = await ArrangeAsync();

            await Assert.ThrowsAsync<DbUpdateException>(
                () => _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = score }));
        }

        [Fact]
        public async Task Get_ReturnsRatingOfThatUser_OrNull()
        {
            var (anime, first, second) = await ArrangeAsync();
            await _repo.AddAsync(new Rating { UserId = first.Id, AnimeId = anime.Id, Score = 7 });

            var found = await _repo.GetAsync(first.Id, anime.Id);
            var missing = await _repo.GetAsync(second.Id, anime.Id);

            Assert.Equal(7, found!.Score);
            Assert.Null(missing);
        }
    }
}
