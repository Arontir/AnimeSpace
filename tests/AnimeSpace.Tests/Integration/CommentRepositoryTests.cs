using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Tests.Integration
{
    /// <summary>
    /// Інтеграційні тести CommentRepository: порядок, ліміт у 100, завантаження авторів.
    /// </summary>
    public class CommentRepositoryTests : IDisposable
    {
        private readonly SqliteTestDb _db = new();
        private readonly CommentRepository _repo;

        public CommentRepositoryTests()
        {
            _repo = new CommentRepository(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public async Task GetByAnimeId_ReturnsNewestFirst_WithAuthors()
        {
            var user = await _db.AddUserAsync("neo");
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            await _db.SeedAsync(
                new Comment { UserId = user.Id, AnimeId = anime.Id, Text = "old", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Comment { UserId = user.Id, AnimeId = anime.Id, Text = "new", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

            var result = await _repo.GetByAnimeIdAsync(anime.Id);

            Assert.Equal(new[] { "new", "old" }, result.Select(c => c.Text));
            Assert.All(result, c => Assert.Equal("neo@mail.com", c.User.UserName));
        }

        [Fact]
        public async Task GetByAnimeId_ReturnsAtMost100Latest()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);

            var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            await _db.SeedRangeAsync(Enumerable.Range(0, 105).Select(i => new Comment
            {
                UserId = user.Id,
                AnimeId = anime.Id,
                Text = $"c{i}",
                CreatedAt = start.AddMinutes(i)
            }));

            var result = await _repo.GetByAnimeIdAsync(anime.Id);

            Assert.Equal(100, result.Count);
            Assert.Equal("c104", result.First().Text);
            Assert.Equal("c5", result.Last().Text);
        }

        [Fact]
        public async Task GetByAnimeId_ReturnsOnlyCommentsOfThatTitle()
        {
            var user = await _db.AddUserAsync();
            var first = TestData.CreateAnime("First");
            var second = TestData.CreateAnime("Second");
            await _db.SeedAsync(first, second);
            await _db.SeedAsync(
                new Comment { UserId = user.Id, AnimeId = first.Id, Text = "a" },
                new Comment { UserId = user.Id, AnimeId = second.Id, Text = "b" });

            var result = await _repo.GetByAnimeIdAsync(first.Id);

            Assert.Equal("a", Assert.Single(result).Text);
        }

        [Fact]
        public async Task GetById_LoadsAuthor()
        {
            var user = await _db.AddUserAsync("neo");
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            var comment = new Comment { UserId = user.Id, AnimeId = anime.Id, Text = "hello" };
            await _repo.AddAsync(comment);
            _db.Context.ChangeTracker.Clear();

            var loaded = await _repo.GetByIdAsync(comment.Id);

            Assert.NotNull(loaded);
            Assert.Equal("neo@mail.com", loaded!.User.UserName);
        }

        [Fact]
        public async Task GetById_UnknownId_ReturnsNull()
        {
            Assert.Null(await _repo.GetByIdAsync(999));
        }

        [Fact]
        public async Task Delete_RemovesComment()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A");
            await _db.SeedAsync(anime);
            var comment = new Comment { UserId = user.Id, AnimeId = anime.Id, Text = "hello" };
            await _repo.AddAsync(comment);

            await _repo.DeleteAsync(comment);

            using var check = _db.CreateNewContext();
            Assert.Equal(0, await check.Comments.CountAsync());
        }
    }
}
