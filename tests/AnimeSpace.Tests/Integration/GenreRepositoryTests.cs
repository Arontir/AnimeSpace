using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Tests.Integration
{
    /// <summary>
    /// Інтеграційні тести GenreRepository: сортування, пошук за ідентифікаторами, унікальність, видалення.
    /// </summary>
    public class GenreRepositoryTests : IDisposable
    {
        private readonly SqliteTestDb _db = new();
        private readonly GenreRepository _repo;

        public GenreRepositoryTests()
        {
            _repo = new GenreRepository(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        private async Task<int> IdOfAsync(string name)
            => await _db.Context.Genres.AsNoTracking().Where(g => g.Name == name).Select(g => g.Id).SingleAsync();

        [Fact]
        public async Task GetAll_ReturnsGenresSortedByName()
        {
            await _db.SeedAsync(
                TestData.CreateGenre("Drama"), TestData.CreateGenre("Action"), TestData.CreateGenre("Comedy"));

            var result = await _repo.GetAllAsync();

            Assert.Equal(new[] { "Action", "Comedy", "Drama" }, result.Select(g => g.Name));
        }

        [Fact]
        public async Task GetByIds_IgnoresUnknownIdsAndDuplicates()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"), TestData.CreateGenre("Drama"));
            var actionId = await IdOfAsync("Action");

            var result = await _repo.GetByIdsAsync(new[] { actionId, actionId, 999 });

            Assert.Equal("Action", Assert.Single(result).Name);
        }

        [Fact]
        public async Task GetByIds_ReturnsTrackedEntities()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"));
            var actionId = await IdOfAsync("Action");

            var result = await _repo.GetByIdsAsync(new[] { actionId });

            Assert.Equal(EntityState.Unchanged, _db.Context.Entry(Assert.Single(result)).State);
        }

        [Fact]
        public async Task GetById_ReturnsTrackedGenre_OrNull()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"));
            var actionId = await IdOfAsync("Action");

            var genre = await _repo.GetByIdAsync(actionId);

            Assert.NotNull(genre);
            Assert.Equal(EntityState.Unchanged, _db.Context.Entry(genre!).State);
            Assert.Null(await _repo.GetByIdAsync(999));
        }

        [Fact]
        public async Task NameExists_ChecksNameAndRespectsExcludedId()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"));
            var actionId = await IdOfAsync("Action");

            Assert.True(await _repo.NameExistsAsync("Action"));
            Assert.False(await _repo.NameExistsAsync("Drama"));

            Assert.False(await _repo.NameExistsAsync("Action", actionId));
            Assert.True(await _repo.NameExistsAsync("Action", actionId + 1));
        }

        [Fact]
        public async Task Add_DuplicateName_ViolatesUniqueIndex()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"));

            await Assert.ThrowsAsync<DbUpdateException>(
                () => _repo.AddAsync(TestData.CreateGenre("Action")));
        }

        [Fact]
        public async Task Update_SavesNewName()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"));
            var genre = await _repo.GetByIdAsync(await IdOfAsync("Action"));

            genre!.Name = "Adventure";
            await _repo.UpdateAsync(genre);

            using var check = _db.CreateNewContext();
            Assert.Equal("Adventure", (await check.Genres.SingleAsync()).Name);
        }

        [Fact]
        public async Task Delete_RemovesGenreAndLinks_ButKeepsTitles()
        {
            var action = TestData.CreateGenre("Action");
            await _db.SeedAsync(TestData.CreateAnime("A", 2020, AnimeSpace.Domain.Enums.AnimeStatus.Finished, action));
            var genre = await _repo.GetByIdAsync(await IdOfAsync("Action"));

            await _repo.DeleteAsync(genre!);

            using var check = _db.CreateNewContext();
            Assert.Equal(0, await check.Genres.CountAsync());
            Assert.Equal(1, await check.Animes.CountAsync());
            Assert.Equal(0, await check.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AnimeGenres").SingleAsync());
        }
    }
}
