using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Services;
using AnimeSpace.Domain.Entities;
using Moq;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Юніт-тести RatingService: перевірки, вибір «створити чи оновити» та результат перерахунку.
    /// </summary>
    public class RatingServiceTests
    {
        private readonly Mock<IRatingRepository> _ratings = new();
        private readonly Mock<IAnimeRepository> _animes = new();
        private readonly RatingService _service;

        public RatingServiceTests()
        {
            _animes.Setup(a => a.ExistsAsync(1)).ReturnsAsync(true);
            _service = new RatingService(_ratings.Object, _animes.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(-3)]
        public async Task SetRating_ScoreOutOfRange_Throws_AndWritesNothing(int score)
        {
            await Assert.ThrowsAsync<ValidationException>(() => _service.SetRatingAsync("u1", 1, score));

            _ratings.Verify(r => r.AddAsync(It.IsAny<Rating>()), Times.Never);
            _ratings.Verify(r => r.UpdateAsync(It.IsAny<Rating>()), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task SetRating_BoundaryScores_AreAccepted(int score)
        {
            _ratings.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync((Rating?)null);

            await _service.SetRatingAsync("u1", 1, score);

            _ratings.Verify(r => r.AddAsync(It.Is<Rating>(x => x.Score == score)), Times.Once);
        }

        [Fact]
        public async Task SetRating_UnknownAnime_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.SetRatingAsync("u1", 99, 5));

            _ratings.Verify(r => r.AddAsync(It.IsAny<Rating>()), Times.Never);
        }

        [Fact]
        public async Task SetRating_NoExistingRating_AddsNewOne()
        {
            _ratings.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync((Rating?)null);

            await _service.SetRatingAsync("u1", 1, 8);

            _ratings.Verify(r => r.AddAsync(
                It.Is<Rating>(x => x.UserId == "u1" && x.AnimeId == 1 && x.Score == 8)), Times.Once);
            _ratings.Verify(r => r.UpdateAsync(It.IsAny<Rating>()), Times.Never);
        }

        [Fact]
        public async Task SetRating_ExistingRating_UpdatesInsteadOfAdding()
        {
            var existing = new Rating { UserId = "u1", AnimeId = 1, Score = 4 };
            _ratings.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync(existing);

            await _service.SetRatingAsync("u1", 1, 9);

            Assert.Equal(9, existing.Score);
            _ratings.Verify(r => r.UpdateAsync(existing), Times.Once);
            _ratings.Verify(r => r.AddAsync(It.IsAny<Rating>()), Times.Never);
        }

        [Fact]
        public async Task SetRating_ReturnsResultOfRecalculation()
        {
            _ratings.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync((Rating?)null);
            _ratings.Setup(r => r.RecalculateAverageRatingAsync(1))
                .ReturnsAsync(new RatingResultDto { AverageRating = 9.0, RatingsCount = 2 });

            var result = await _service.SetRatingAsync("u1", 1, 8);

            Assert.Equal(9.0, result.AverageRating);
            Assert.Equal(2, result.RatingsCount);
        }

        [Fact]
        public async Task SetRating_RecalculatesOnlyAfterTheRatingIsSaved()
        {
            var calls = new List<string>();
            _ratings.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync((Rating?)null);
            _ratings.Setup(r => r.AddAsync(It.IsAny<Rating>()))
                .Callback(() => calls.Add("add"))
                .Returns(Task.CompletedTask);
            _ratings.Setup(r => r.RecalculateAverageRatingAsync(1))
                .Callback(() => calls.Add("recalculate"))
                .ReturnsAsync(new RatingResultDto { AverageRating = 8, RatingsCount = 1 });

            await _service.SetRatingAsync("u1", 1, 8);

            Assert.Equal(new[] { "add", "recalculate" }, calls);
        }

        [Fact]
        public async Task GetUserRating_ReturnsScore_OrNullWhenNotRated()
        {
            _ratings.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync(new Rating { UserId = "u1", AnimeId = 1, Score = 7 });
            _ratings.Setup(r => r.GetAsync("u2", 1)).ReturnsAsync((Rating?)null);

            Assert.Equal(7, await _service.GetUserRatingAsync("u1", 1));
            Assert.Null(await _service.GetUserRatingAsync("u2", 1));
        }
    }
}
