using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Services;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using Moq;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Юніт-тести BookmarkService. Головне: повторне додавання не створює дублікат.
    /// </summary>
    public class BookmarkServiceTests
    {
        private readonly Mock<IBookmarkRepository> _bookmarks = new();
        private readonly Mock<IAnimeRepository> _animes = new();
        private readonly BookmarkService _service;

        public BookmarkServiceTests()
        {
            _animes.Setup(a => a.ExistsAsync(1)).ReturnsAsync(true);
            _service = new BookmarkService(_bookmarks.Object, _animes.Object);
        }

        [Fact]
        public async Task SetStatus_NoBookmark_AddsNewOne()
        {
            _bookmarks.Setup(b => b.GetAsync("u1", 1)).ReturnsAsync((Bookmark?)null);

            await _service.SetStatusAsync("u1", 1, WatchStatus.Watching);

            _bookmarks.Verify(b => b.AddAsync(It.Is<Bookmark>(x =>
                x.UserId == "u1" && x.AnimeId == 1 && x.Status == WatchStatus.Watching)), Times.Once);
            _bookmarks.Verify(b => b.UpdateAsync(It.IsAny<Bookmark>()), Times.Never);
        }

        [Fact]
        public async Task SetStatus_ExistingBookmark_UpdatesAndDoesNotAddSecondRecord()
        {
            var oldDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var existing = new Bookmark { UserId = "u1", AnimeId = 1, Status = WatchStatus.Planned, UpdatedAt = oldDate };
            _bookmarks.Setup(b => b.GetAsync("u1", 1)).ReturnsAsync(existing);

            await _service.SetStatusAsync("u1", 1, WatchStatus.Completed);

            Assert.Equal(WatchStatus.Completed, existing.Status);
            Assert.True(existing.UpdatedAt > oldDate);
            _bookmarks.Verify(b => b.UpdateAsync(existing), Times.Once);
            _bookmarks.Verify(b => b.AddAsync(It.IsAny<Bookmark>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(-1)]
        public async Task SetStatus_UnknownStatus_Throws(int value)
        {
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.SetStatusAsync("u1", 1, (WatchStatus)value));

            _bookmarks.Verify(b => b.AddAsync(It.IsAny<Bookmark>()), Times.Never);
        }

        [Fact]
        public async Task SetStatus_UnknownAnime_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.SetStatusAsync("u1", 99, WatchStatus.Planned));

            _bookmarks.Verify(b => b.AddAsync(It.IsAny<Bookmark>()), Times.Never);
        }

        [Fact]
        public async Task GetStatus_ReturnsStatus_OrNullWhenNoBookmark()
        {
            _bookmarks.Setup(b => b.GetAsync("u1", 1))
                .ReturnsAsync(new Bookmark { UserId = "u1", AnimeId = 1, Status = WatchStatus.Dropped });
            _bookmarks.Setup(b => b.GetAsync("u2", 1)).ReturnsAsync((Bookmark?)null);

            Assert.Equal(WatchStatus.Dropped, await _service.GetStatusAsync("u1", 1));
            Assert.Null(await _service.GetStatusAsync("u2", 1));
        }

        [Fact]
        public async Task GetUserBookmarks_ReturnsMostRecentlyChangedFirst_WithAnimeTitle()
        {
            var older = new Bookmark
            {
                UserId = "u1", AnimeId = 1, Status = WatchStatus.Planned,
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Anime = new Anime { Title = "Older" }
            };
            var newer = new Bookmark
            {
                UserId = "u1", AnimeId = 2, Status = WatchStatus.Watching,
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Anime = new Anime { Title = "Newer" }
            };
            _bookmarks.Setup(b => b.GetByUserIdAsync("u1")).ReturnsAsync(new List<Bookmark> { older, newer });

            var result = await _service.GetUserBookmarksAsync("u1");

            Assert.Equal(new[] { "Newer", "Older" }, result.Select(b => b.AnimeTitle));
        }

        [Fact]
        public async Task Delete_ExistingBookmark_IsRemoved()
        {
            var existing = new Bookmark { UserId = "u1", AnimeId = 1 };
            _bookmarks.Setup(b => b.GetAsync("u1", 1)).ReturnsAsync(existing);

            await _service.DeleteAsync("u1", 1);

            _bookmarks.Verify(b => b.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task Delete_MissingBookmark_ThrowsNotFound()
        {
            _bookmarks.Setup(b => b.GetAsync("u1", 1)).ReturnsAsync((Bookmark?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync("u1", 1));
        }
    }
}
