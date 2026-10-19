using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Tests.Integration
{
    /// <summary>
    /// Інтеграційні тести BookmarkRepository: унікальність пари, вибірка списку користувача, зміна й видалення.
    /// </summary>
    public class BookmarkRepositoryTests : IDisposable
    {
        private readonly SqliteTestDb _db = new();
        private readonly BookmarkRepository _repo;

        public BookmarkRepositoryTests()
        {
            _repo = new BookmarkRepository(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public async Task Add_SecondBookmarkForTheSamePair_ViolatesUniqueIndex()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            await _repo.AddAsync(new Bookmark { UserId = user.Id, AnimeId = anime.Id });

            await Assert.ThrowsAsync<DbUpdateException>(
                () => _repo.AddAsync(new Bookmark { UserId = user.Id, AnimeId = anime.Id }));
        }

        [Fact]
        public async Task Add_SameTitleForDifferentUsers_IsAllowed()
        {
            var first = await _db.AddUserAsync("one");
            var second = await _db.AddUserAsync("two");
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);

            await _repo.AddAsync(new Bookmark { UserId = first.Id, AnimeId = anime.Id });
            await _repo.AddAsync(new Bookmark { UserId = second.Id, AnimeId = anime.Id });

            using var check = _db.CreateNewContext();
            Assert.Equal(2, await check.Bookmarks.CountAsync());
        }

        [Fact]
        public async Task GetByUserId_ReturnsOnlyThatUsersBookmarks_WithTitles()
        {
            var first = await _db.AddUserAsync("one");
            var second = await _db.AddUserAsync("two");
            var bebop = TestData.CreateAnime("Bebop");
            var haikyu = TestData.CreateAnime("Haikyu");
            await _db.SeedAsync(bebop, haikyu);
            await _db.SeedAsync(
                new Bookmark { UserId = first.Id, AnimeId = bebop.Id, Status = WatchStatus.Watching },
                new Bookmark { UserId = second.Id, AnimeId = haikyu.Id });

            var result = await _repo.GetByUserIdAsync(first.Id);

            var bookmark = Assert.Single(result);
            Assert.Equal("Bebop", bookmark.Anime.Title);
            Assert.Equal(WatchStatus.Watching, bookmark.Status);
        }

        [Fact]
        public async Task Get_ReturnsBookmarkOrNull()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            await _repo.AddAsync(new Bookmark { UserId = user.Id, AnimeId = anime.Id });

            Assert.NotNull(await _repo.GetAsync(user.Id, anime.Id));
            Assert.Null(await _repo.GetAsync(user.Id, 999));
        }

        [Fact]
        public async Task Update_SavesChangedStatus()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            await _repo.AddAsync(new Bookmark { UserId = user.Id, AnimeId = anime.Id, Status = WatchStatus.Planned });

            var bookmark = await _repo.GetAsync(user.Id, anime.Id);
            bookmark!.Status = WatchStatus.Completed;
            await _repo.UpdateAsync(bookmark);

            using var check = _db.CreateNewContext();
            var saved = await check.Bookmarks.AsNoTracking().SingleAsync();
            Assert.Equal(WatchStatus.Completed, saved.Status);
        }

        [Fact]
        public async Task Delete_RemovesBookmark()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            await _repo.AddAsync(new Bookmark { UserId = user.Id, AnimeId = anime.Id });

            var bookmark = await _repo.GetAsync(user.Id, anime.Id);
            await _repo.DeleteAsync(bookmark!);

            using var check = _db.CreateNewContext();
            Assert.Equal(0, await check.Bookmarks.CountAsync());
        }
    }
}
