using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Tests.Integration
{
    /// <summary>
    /// Інтеграційні тести AnimeRepository на справжньому EF Core (SQLite у пам'яті):
    /// перевіряють реальні запити, фільтри, пагінацію, відстеження змін і каскадне видалення.
    /// </summary>
    public class AnimeRepositoryTests : IDisposable
    {
        private readonly SqliteTestDb _db = new();
        private readonly AnimeRepository _repo;

        public AnimeRepositoryTests()
        {
            _repo = new AnimeRepository(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        /// <summary>
        /// Три тайтли: Bebop (Action), Haikyu (Sports), Frieren (Action + Sports, онгоїнг).
        /// </summary>
        private async Task SeedBasicAsync()
        {
            var action = TestData.CreateGenre("Action");
            var sports = TestData.CreateGenre("Sports");

            await _db.SeedAsync(
                TestData.CreateAnime("Cowboy Bebop", 1998, AnimeStatus.Finished, action),
                TestData.CreateAnime("Haikyu", 2014, AnimeStatus.Finished, sports),
                TestData.CreateAnime("Frieren", 2023, AnimeStatus.Ongoing, action, sports));
        }

        private async Task<int> IdOfAsync(string title)
            => await _db.Context.Animes.AsNoTracking().Where(a => a.Title == title).Select(a => a.Id).SingleAsync();

        private async Task<int> GenreIdAsync(string name)
            => await _db.Context.Genres.AsNoTracking().Where(g => g.Name == name).Select(g => g.Id).SingleAsync();

        private static Anime Rated(string title, double average, int count)
        {
            var anime = TestData.CreateAnime(title);
            anime.AverageRating = average;
            anime.RatingsCount = count;
            return anime;
        }

        [Fact]
        public async Task GetFiltered_ByGenre_ReturnsOnlyTitlesWithThatGenre()
        {
            await SeedBasicAsync();
            var actionId = await GenreIdAsync("Action");

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { GenreId = actionId });

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(new[] { "Cowboy Bebop", "Frieren" }, result.Items.Select(a => a.Title).Order());
        }

        [Fact]
        public async Task GetFiltered_ByYear_ReturnsOnlyThatYear()
        {
            await SeedBasicAsync();

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { ReleaseYear = 2014 });

            Assert.Equal("Haikyu", Assert.Single(result.Items).Title);
        }

        [Fact]
        public async Task GetFiltered_ByStatus_ReturnsOnlyThatStatus()
        {
            await SeedBasicAsync();

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { Status = AnimeStatus.Ongoing });

            Assert.Equal("Frieren", Assert.Single(result.Items).Title);
        }

        [Fact]
        public async Task GetFiltered_GenreAndStatusTogether_AreCombinedWithAnd()
        {
            await SeedBasicAsync();
            var actionId = await GenreIdAsync("Action");

            var result = await _repo.GetFilteredAsync(
                new AnimeFilterDto { GenreId = actionId, Status = AnimeStatus.Finished });

            Assert.Equal("Cowboy Bebop", Assert.Single(result.Items).Title);
        }

        [Fact]
        public async Task GetFiltered_NoFilters_ReturnsEverything()
        {
            await SeedBasicAsync();

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto());

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public async Task GetFiltered_SearchQuery_FindsPartOfTitle_IgnoringCase()
        {
            await SeedBasicAsync();

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SearchQuery = "BOP" });

            Assert.Equal("Cowboy Bebop", Assert.Single(result.Items).Title);
        }

        [Fact]
        public async Task GetFiltered_SearchQueryWithPercent_IsTreatedAsPlainText()
        {
            await _db.SeedAsync(TestData.CreateAnime("100% Sure"), TestData.CreateAnime("Other"));

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SearchQuery = "%" });

            Assert.Equal("100% Sure", Assert.Single(result.Items).Title);
        }

        [Fact]
        public async Task GetFiltered_SearchQueryWithUnderscore_IsTreatedAsPlainText()
        {
            await _db.SeedAsync(TestData.CreateAnime("a_b"), TestData.CreateAnime("axb"));

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SearchQuery = "a_b" });

            Assert.Equal("a_b", Assert.Single(result.Items).Title);
        }

        private async Task SeedTwentyAsync()
            => await _db.SeedRangeAsync(Enumerable.Range(1, 20).Select(i => TestData.CreateAnime($"Anime {i:00}")));

        [Fact]
        public async Task GetFiltered_Pagination_FirstPageHasRequestedSize()
        {
            await SeedTwentyAsync();

            var result = await _repo.GetFilteredAsync(
                new AnimeFilterDto { Page = 1, PageSize = 8, SortBy = AnimeSortBy.TitleAsc });

            Assert.Equal(8, result.Items.Count);
            Assert.Equal(20, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(8, result.PageSize);
            Assert.Equal(
                Enumerable.Range(1, 8).Select(i => $"Anime {i:00}"),
                result.Items.Select(a => a.Title));
        }

        [Fact]
        public async Task GetFiltered_Pagination_LastPageHasTheRemainder()
        {
            await SeedTwentyAsync();

            var result = await _repo.GetFilteredAsync(
                new AnimeFilterDto { Page = 3, PageSize = 8, SortBy = AnimeSortBy.TitleAsc });

            Assert.Equal(4, result.Items.Count);
            Assert.Equal("Anime 17", result.Items.First().Title);
            Assert.Equal("Anime 20", result.Items.Last().Title);
        }

        [Fact]
        public async Task GetFiltered_Pagination_PageBeyondTheEnd_IsEmptyButKeepsTotal()
        {
            await SeedTwentyAsync();

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { Page = 10, PageSize = 8 });

            Assert.Empty(result.Items);
            Assert.Equal(20, result.TotalCount);
        }

        [Fact]
        public async Task GetFiltered_Pagination_PagesDoNotOverlapOrLoseItems_EvenWithEqualSortKeys()
        {
            await _db.SeedRangeAsync(Enumerable.Range(1, 20).Select(i => TestData.CreateAnime($"A{i}", 2020)));

            var ids = new List<int>();
            for (var page = 1; page <= 3; page++)
            {
                var result = await _repo.GetFilteredAsync(
                    new AnimeFilterDto { Page = page, PageSize = 8, SortBy = AnimeSortBy.ReleaseYearDesc });
                ids.AddRange(result.Items.Select(a => a.Id));
            }

            Assert.Equal(20, ids.Count);
            Assert.Equal(20, ids.Distinct().Count());
        }

        [Fact]
        public async Task GetFiltered_SortByTitle_OrdersAlphabetically()
        {
            await _db.SeedAsync(TestData.CreateAnime("Charlie"), TestData.CreateAnime("Alpha"), TestData.CreateAnime("Bravo"));

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SortBy = AnimeSortBy.TitleAsc });

            Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, result.Items.Select(a => a.Title));
        }

        [Fact]
        public async Task GetFiltered_SortByReleaseYear_NewestYearFirst()
        {
            await _db.SeedAsync(
                TestData.CreateAnime("Old", 1998), TestData.CreateAnime("New", 2023), TestData.CreateAnime("Mid", 2014));

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SortBy = AnimeSortBy.ReleaseYearDesc });

            Assert.Equal(new[] { "New", "Mid", "Old" }, result.Items.Select(a => a.Title));
        }

        [Fact]
        public async Task GetFiltered_SortByTopRated_AverageFirst_ThenCount()
        {
            await _db.SeedAsync(Rated("Low", 7.0, 50), Rated("MidFew", 9.0, 2), Rated("MidMany", 9.0, 5));

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SortBy = AnimeSortBy.TopRated });

            Assert.Equal(new[] { "MidMany", "MidFew", "Low" }, result.Items.Select(a => a.Title));
        }

        [Fact]
        public async Task GetFiltered_SortByNewest_LatestCreatedFirst()
        {
            var first = TestData.CreateAnime("First");
            first.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var second = TestData.CreateAnime("Second");
            second.CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            await _db.SeedAsync(first, second);

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SortBy = AnimeSortBy.Newest });

            Assert.Equal(new[] { "Second", "First" }, result.Items.Select(a => a.Title));
        }

        [Fact]
        public async Task GetFiltered_LoadsGenresOfEachItem()
        {
            await SeedBasicAsync();

            var result = await _repo.GetFilteredAsync(new AnimeFilterDto { SearchQuery = "Frieren" });

            var anime = Assert.Single(result.Items);
            Assert.Equal(new[] { "Action", "Sports" }, anime.Genres.Select(g => g.Name).Order());
        }

        [Fact]
        public async Task GetLatest_ReturnsNewestFirst_LimitedByCount()
        {
            var old = TestData.CreateAnime("Old");
            old.CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var mid = TestData.CreateAnime("Mid");
            mid.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var recent = TestData.CreateAnime("Recent");
            recent.CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            await _db.SeedAsync(old, mid, recent);

            var result = await _repo.GetLatestAsync(2);

            Assert.Equal(new[] { "Recent", "Mid" }, result.Select(a => a.Title));
        }

        [Fact]
        public async Task GetTopRated_OrdersByAverageThenCount_AndLoadsGenres()
        {
            var action = TestData.CreateGenre("Action");
            var best = Rated("Best", 9.5, 3);
            best.Genres.Add(action);
            await _db.SeedAsync(Rated("Low", 7.0, 50), best, Rated("Mid", 9.0, 5));

            var result = await _repo.GetTopRatedAsync(2);

            Assert.Equal(new[] { "Best", "Mid" }, result.Select(a => a.Title));
            Assert.Single(result[0].Genres);
        }

        [Fact]
        public async Task GetById_ReturnsTrackedEntity_WithoutGenres()
        {
            await SeedBasicAsync();

            var anime = await _repo.GetByIdAsync(await IdOfAsync("Frieren"));

            Assert.NotNull(anime);
            Assert.Equal(EntityState.Unchanged, _db.Context.Entry(anime!).State);
            Assert.Empty(anime!.Genres);
        }

        [Fact]
        public async Task GetForUpdate_ReturnsTrackedEntity_WithGenres()
        {
            await SeedBasicAsync();

            var anime = await _repo.GetForUpdateAsync(await IdOfAsync("Frieren"));

            Assert.NotNull(anime);
            Assert.Equal(2, anime!.Genres.Count);
            Assert.Equal(EntityState.Unchanged, _db.Context.Entry(anime).State);
        }

        [Fact]
        public async Task GetDetails_ReturnsUntrackedEntity_WithGenres()
        {
            await SeedBasicAsync();

            var anime = await _repo.GetDetailsAsync(await IdOfAsync("Frieren"));

            Assert.NotNull(anime);
            Assert.Equal(2, anime!.Genres.Count);
            Assert.Equal(EntityState.Detached, _db.Context.Entry(anime).State);
        }

        [Fact]
        public async Task GetDetails_UnknownId_ReturnsNull()
        {
            Assert.Null(await _repo.GetDetailsAsync(999));
        }

        [Fact]
        public async Task Exists_ReflectsPresenceOfTheTitle()
        {
            await SeedBasicAsync();
            var id = await IdOfAsync("Haikyu");

            Assert.True(await _repo.ExistsAsync(id));
            Assert.False(await _repo.ExistsAsync(999));
        }

        [Fact]
        public async Task Add_WithExistingGenres_DoesNotDuplicateGenres()
        {
            await _db.SeedAsync(TestData.CreateGenre("Action"));

            var genres = await _db.Context.Genres.ToListAsync();
            var anime = TestData.CreateAnime("New", 2020, AnimeStatus.Finished, genres.ToArray());

            await _repo.AddAsync(anime);

            using var check = _db.CreateNewContext();
            Assert.NotEqual(0, anime.Id);
            Assert.Equal(1, await check.Genres.CountAsync());
            var saved = await check.Animes.Include(a => a.Genres).SingleAsync();
            Assert.Equal("Action", Assert.Single(saved.Genres).Name);
        }

        [Fact]
        public async Task Update_SavesChangesOfTrackedEntity()
        {
            await SeedBasicAsync();
            var id = await IdOfAsync("Haikyu");

            var anime = await _repo.GetForUpdateAsync(id);
            anime!.Title = "Haikyu!!";
            anime.PlayerUrl = "https://example.com/player";
            await _repo.UpdateAsync(anime);

            using var check = _db.CreateNewContext();
            var saved = await check.Animes.AsNoTracking().SingleAsync(a => a.Id == id);
            Assert.Equal("Haikyu!!", saved.Title);
            Assert.Equal("https://example.com/player", saved.PlayerUrl);
        }

        [Fact]
        public async Task Update_DoesNotOverwriteRatingChangedByAnotherRequest()
        {
            await _db.SeedAsync(TestData.CreateAnime("Bebop"));
            var id = await IdOfAsync("Bebop");

            var anime = await _repo.GetForUpdateAsync(id);

            using (var other = _db.CreateNewContext())
            {
                await other.Animes.Where(a => a.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.AverageRating, 8.2));
            }

            anime!.Title = "Bebop (renamed)";
            await _repo.UpdateAsync(anime);

            using var check = _db.CreateNewContext();
            var saved = await check.Animes.AsNoTracking().SingleAsync(a => a.Id == id);
            Assert.Equal("Bebop (renamed)", saved.Title);
            Assert.Equal(8.2, saved.AverageRating);
        }

        [Fact]
        public async Task Update_ReplacesGenres()
        {
            var action = TestData.CreateGenre("Action");
            var drama = TestData.CreateGenre("Drama");
            await _db.SeedAsync(TestData.CreateAnime("A", 2020, AnimeStatus.Finished, action), drama);
            var id = await IdOfAsync("A");

            var anime = await _repo.GetForUpdateAsync(id);
            var dramaTracked = await _db.Context.Genres.SingleAsync(g => g.Name == "Drama");
            anime!.Genres.Clear();
            anime.Genres.Add(dramaTracked);
            await _repo.UpdateAsync(anime);

            using var check = _db.CreateNewContext();
            var saved = await check.Animes.Include(a => a.Genres).SingleAsync(a => a.Id == id);
            Assert.Equal(new[] { "Drama" }, saved.Genres.Select(g => g.Name));
            Assert.Equal(2, await check.Genres.CountAsync());
        }

        [Fact]
        public async Task Delete_RemovesTitleAndItsRelatedData_ButKeepsGenres()
        {
            var user = await _db.AddUserAsync();
            var anime = TestData.CreateAnime("A", 2020, AnimeStatus.Finished, TestData.CreateGenre("Action"));
            await _db.SeedAsync(anime);
            await _db.SeedAsync(
                new Rating { UserId = user.Id, AnimeId = anime.Id, Score = 5 },
                new Bookmark { UserId = user.Id, AnimeId = anime.Id },
                new Comment { UserId = user.Id, AnimeId = anime.Id, Text = "x" });

            var tracked = await _repo.GetByIdAsync(anime.Id);
            await _repo.DeleteAsync(tracked!);

            using var check = _db.CreateNewContext();
            Assert.Equal(0, await check.Animes.CountAsync());
            Assert.Equal(0, await check.Ratings.CountAsync());
            Assert.Equal(0, await check.Bookmarks.CountAsync());
            Assert.Equal(0, await check.Comments.CountAsync());
            Assert.Equal(1, await check.Genres.CountAsync());
            Assert.Equal(0, await check.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AnimeGenres").SingleAsync());
        }
    }
}
