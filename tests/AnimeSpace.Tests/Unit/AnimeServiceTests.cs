using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Storage;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Services;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Tests.Helpers;
using Moq;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Юніт-тести AnimeService. Репозиторії й сховище файлів замінені моками (Moq): бази й диска немає.
    /// </summary>
    public class AnimeServiceTests
    {
        private readonly Mock<IAnimeRepository> _animes = new();
        private readonly Mock<IGenreRepository> _genres = new();
        private readonly Mock<IFileService> _files = new();
        private readonly AnimeService _service;

        /// <summary>
        /// Останній тайтл, переданий у AddAsync (див. <see cref="CaptureAddedAnime"/>).
        /// </summary>
        private Anime? _addedAnime;

        public AnimeServiceTests()
        {
            _service = new AnimeService(_animes.Object, _genres.Object, _files.Object);
        }

        private static PagedResult<Anime> Page(params Anime[] items) => new()
        {
            Items = items,
            TotalCount = items.Length,
            PageNumber = 1,
            PageSize = 12
        };

        /// <summary>
        /// Викликає каталог і повертає фільтр, який сервіс передав у репозиторій.
        /// </summary>
        private async Task<AnimeFilterDto> CaptureFilterAsync(AnimeFilterDto input)
        {
            AnimeFilterDto? captured = null;

            _animes.Setup(r => r.GetFilteredAsync(It.IsAny<AnimeFilterDto>()))
                .Callback<AnimeFilterDto>(f => captured = f)
                .ReturnsAsync(Page());

            await _service.GetCatalogAsync(input);

            return captured!;
        }

        /// <summary>
        /// Налаштовує AddAsync: запам'ятовує тайтл і імітує присвоєння Id базою.
        /// </summary>
        private void CaptureAddedAnime(int newId = 1)
        {
            _animes.Setup(r => r.AddAsync(It.IsAny<Anime>()))
                .Callback<Anime>(a =>
                {
                    a.Id = newId;
                    _addedAnime = a;
                })
                .Returns(Task.CompletedTask);
        }

        private static FileUploadDto NewUpload() => new()
        {
            Content = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "poster.jpg"
        };

        /// <summary>
        /// Тайтл із Id = 7, який репозиторій повертає для редагування.
        /// </summary>
        private Anime ExistingAnime(string? posterUrl = null, string? playerUrl = null)
        {
            var anime = new Anime
            {
                Id = 7,
                Title = "Old",
                ReleaseYear = 2000,
                PosterUrl = posterUrl,
                PlayerUrl = playerUrl
            };

            _animes.Setup(r => r.GetForUpdateAsync(7)).ReturnsAsync(anime);
            return anime;
        }

        private static AnimeUpdateDto UpdateDto() => new()
        {
            Id = 7,
            Title = "New",
            ReleaseYear = 2001
        };

        [Fact]
        public async Task GetCatalog_HugePageSize_IsClampedTo48()
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto { PageSize = 1_000_000 });

            Assert.Equal(48, filter.PageSize);
        }

        [Fact]
        public async Task GetCatalog_ZeroPageSize_BecomesOne()
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto { PageSize = 0 });

            Assert.Equal(1, filter.PageSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetCatalog_PageBelowOne_BecomesOne(int page)
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto { Page = page });

            Assert.Equal(1, filter.Page);
        }

        [Fact]
        public async Task GetCatalog_DoesNotModifyInputFilter()
        {
            var input = new AnimeFilterDto { Page = -5, PageSize = 1_000_000, SearchQuery = "  bebop  " };

            await CaptureFilterAsync(input);

            Assert.Equal(-5, input.Page);
            Assert.Equal(1_000_000, input.PageSize);
            Assert.Equal("  bebop  ", input.SearchQuery);
        }

        [Theory]
        [InlineData("  bebop  ", "bebop")]
        [InlineData("   ", null)]
        [InlineData("", null)]
        public async Task GetCatalog_SearchQuery_IsTrimmedAndEmptyBecomesNull(string input, string? expected)
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto { SearchQuery = input });

            Assert.Equal(expected, filter.SearchQuery);
        }

        [Fact]
        public async Task GetCatalog_UnknownSortBy_FallsBackToNewest()
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto { SortBy = (AnimeSortBy)99 });

            Assert.Equal(AnimeSortBy.Newest, filter.SortBy);
        }

        [Fact]
        public async Task GetCatalog_UnknownStatus_IsIgnored()
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto { Status = (AnimeStatus)99 });

            Assert.Null(filter.Status);
        }

        [Fact]
        public async Task GetCatalog_ValidFilters_ArePassedThrough()
        {
            var filter = await CaptureFilterAsync(new AnimeFilterDto
            {
                GenreId = 3,
                ReleaseYear = 2020,
                Status = AnimeStatus.Ongoing,
                SortBy = AnimeSortBy.TopRated,
                Page = 2,
                PageSize = 8
            });

            Assert.Equal(3, filter.GenreId);
            Assert.Equal(2020, filter.ReleaseYear);
            Assert.Equal(AnimeStatus.Ongoing, filter.Status);
            Assert.Equal(AnimeSortBy.TopRated, filter.SortBy);
            Assert.Equal(2, filter.Page);
            Assert.Equal(8, filter.PageSize);
        }

        [Fact]
        public async Task GetCatalog_ReturnsPagingFromRepository()
        {
            _animes.Setup(r => r.GetFilteredAsync(It.IsAny<AnimeFilterDto>()))
                .ReturnsAsync(new PagedResult<Anime>
                {
                    Items = new List<Anime> { TestData.CreateAnime("A") },
                    TotalCount = 20,
                    PageNumber = 2,
                    PageSize = 8
                });

            var result = await _service.GetCatalogAsync(new AnimeFilterDto());

            Assert.Equal(20, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(8, result.PageSize);
            Assert.Equal(3, result.TotalPages);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetCatalog_MapsItemsToCards_WithSortedGenreNames()
        {
            var anime = TestData.CreateAnime("A", 2020, AnimeStatus.Finished,
                TestData.CreateGenre("Drama"), TestData.CreateGenre("Action"));

            _animes.Setup(r => r.GetFilteredAsync(It.IsAny<AnimeFilterDto>())).ReturnsAsync(Page(anime));

            var result = await _service.GetCatalogAsync(new AnimeFilterDto());

            var card = Assert.Single(result.Items);
            Assert.Equal("A", card.Title);
            Assert.Equal(new[] { "Action", "Drama" }, card.GenreNames);
        }

        [Theory]
        [InlineData(1000, 20)]
        [InlineData(0, 1)]
        [InlineData(5, 5)]
        public async Task GetLatest_ClampsCount(int requested, int expected)
        {
            _animes.Setup(r => r.GetLatestAsync(It.IsAny<int>())).ReturnsAsync(new List<Anime>());

            await _service.GetLatestAsync(requested);

            _animes.Verify(r => r.GetLatestAsync(expected), Times.Once);
        }

        [Theory]
        [InlineData(1000, 20)]
        [InlineData(-3, 1)]
        [InlineData(10, 10)]
        public async Task GetTopRated_ClampsCount(int requested, int expected)
        {
            _animes.Setup(r => r.GetTopRatedAsync(It.IsAny<int>())).ReturnsAsync(new List<Anime>());

            await _service.GetTopRatedAsync(requested);

            _animes.Verify(r => r.GetTopRatedAsync(expected), Times.Once);
        }

        [Fact]
        public async Task GetDetails_UnknownId_ThrowsNotFound()
        {
            _animes.Setup(r => r.GetDetailsAsync(5)).ReturnsAsync((Anime?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetDetailsAsync(5));
        }

        [Fact]
        public async Task GetDetails_ReturnsMappedDto()
        {
            var anime = TestData.CreateAnime("Bebop", 1998, AnimeStatus.Finished, TestData.CreateGenre("Action"));
            anime.Id = 5;
            anime.PlayerUrl = "https://example.com/player";
            _animes.Setup(r => r.GetDetailsAsync(5)).ReturnsAsync(anime);

            var dto = await _service.GetDetailsAsync(5);

            Assert.Equal(5, dto.Id);
            Assert.Equal("Bebop", dto.Title);
            Assert.Equal("https://example.com/player", dto.PlayerUrl);
            Assert.Single(dto.Genres);
        }

        [Fact]
        public async Task GetForEdit_UnknownId_ThrowsNotFound()
        {
            _animes.Setup(r => r.GetDetailsAsync(5)).ReturnsAsync((Anime?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetForEditAsync(5));
        }

        [Fact]
        public async Task GetForEdit_ReturnsSelectedGenreIds()
        {
            var action = TestData.CreateGenre("Action");
            action.Id = 4;
            var anime = TestData.CreateAnime("A", 2020, AnimeStatus.Finished, action);
            anime.Id = 5;
            _animes.Setup(r => r.GetDetailsAsync(5)).ReturnsAsync(anime);

            var dto = await _service.GetForEditAsync(5);

            Assert.Equal(5, dto.Id);
            Assert.Equal(new[] { 4 }, dto.SelectedGenreIds);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_BlankTitle_Throws_AndWritesNothing(string title)
        {
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateAsync(new AnimeCreateDto { Title = title }, NewUpload()));

            _animes.Verify(r => r.AddAsync(It.IsAny<Anime>()), Times.Never);
            _files.Verify(f => f.SavePosterAsync(It.IsAny<FileUploadDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_TooLongTitle_Throws()
        {
            var dto = new AnimeCreateDto { Title = new string('a', 201) };

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto, null));
        }

        [Fact]
        public async Task Create_TrimsTitle()
        {
            CaptureAddedAnime();

            await _service.CreateAsync(new AnimeCreateDto { Title = "  Bebop  " }, null);

            Assert.Equal("Bebop", _addedAnime!.Title);
        }

        [Fact]
        public async Task Create_PlayerUrlWithoutScheme_IsNormalizedToHttps()
        {
            CaptureAddedAnime();

            await _service.CreateAsync(
                new AnimeCreateDto { Title = "A", PlayerUrl = "//kodik.info/video/1/abc/720p" }, null);

            Assert.Equal("https://kodik.info/video/1/abc/720p", _addedAnime!.PlayerUrl);
        }

        [Theory]
        [InlineData("http://example.com/x")]
        [InlineData("ftp://example.com/x")]
        [InlineData("javascript:alert(1)")]
        [InlineData("not a url")]
        public async Task Create_InvalidPlayerUrl_Throws_BeforeAnythingIsSaved(string url)
        {
            var dto = new AnimeCreateDto { Title = "A", PlayerUrl = url };

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto, NewUpload()));

            _files.Verify(f => f.SavePosterAsync(It.IsAny<FileUploadDto>()), Times.Never);
            _animes.Verify(r => r.AddAsync(It.IsAny<Anime>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_EmptyPlayerUrl_BecomesNull(string url)
        {
            CaptureAddedAnime();

            await _service.CreateAsync(new AnimeCreateDto { Title = "A", PlayerUrl = url }, null);

            Assert.Null(_addedAnime!.PlayerUrl);
        }

        [Theory]
        [InlineData("javascript:alert(1)")]
        [InlineData("ftp://example.com/p.jpg")]
        [InlineData("/uploads/posters/other.jpg")]
        public async Task Create_InvalidExternalPosterUrl_Throws(string url)
        {
            var dto = new AnimeCreateDto { Title = "A", PosterUrl = url };

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto, null));
        }

        [Fact]
        public async Task Create_ExternalPosterUrl_IsKept()
        {
            CaptureAddedAnime();

            await _service.CreateAsync(
                new AnimeCreateDto { Title = "A", PosterUrl = "https://example.com/p.jpg" }, null);

            Assert.Equal("https://example.com/p.jpg", _addedAnime!.PosterUrl);
            _files.Verify(f => f.SavePosterAsync(It.IsAny<FileUploadDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_UploadedPoster_HasPriorityOverExternalUrl()
        {
            CaptureAddedAnime();
            _files.Setup(f => f.SavePosterAsync(It.IsAny<FileUploadDto>())).ReturnsAsync("/uploads/posters/new.jpg");

            await _service.CreateAsync(
                new AnimeCreateDto { Title = "A", PosterUrl = "https://example.com/p.jpg" }, NewUpload());

            Assert.Equal("/uploads/posters/new.jpg", _addedAnime!.PosterUrl);
        }

        [Fact]
        public async Task Create_UnknownGenre_Throws()
        {
            var action = TestData.CreateGenre("Action");
            action.Id = 1;

            _genres.Setup(g => g.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Genre> { action });

            var dto = new AnimeCreateDto { Title = "A", SelectedGenreIds = [1, 2] };

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto, null));
            _animes.Verify(r => r.AddAsync(It.IsAny<Anime>()), Times.Never);
        }

        [Fact]
        public async Task Create_AddsSelectedGenres_UsingTheSameObjectsFromRepository()
        {
            CaptureAddedAnime();

            var action = TestData.CreateGenre("Action");
            action.Id = 1;
            var drama = TestData.CreateGenre("Drama");
            drama.Id = 2;

            _genres.Setup(g => g.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Genre> { action, drama });

            await _service.CreateAsync(new AnimeCreateDto { Title = "A", SelectedGenreIds = [1, 2, 2] }, null);

            Assert.Equal(2, _addedAnime!.Genres.Count);
            Assert.Contains(action, _addedAnime.Genres);
            Assert.Contains(drama, _addedAnime.Genres);
        }

        [Fact]
        public async Task Create_ReturnsIdAssignedByRepository()
        {
            CaptureAddedAnime(newId: 42);

            var id = await _service.CreateAsync(new AnimeCreateDto { Title = "A" }, null);

            Assert.Equal(42, id);
        }

        [Fact]
        public async Task Create_WhenDatabaseFails_DeletesJustSavedPoster()
        {
            _files.Setup(f => f.SavePosterAsync(It.IsAny<FileUploadDto>())).ReturnsAsync("/uploads/posters/new.jpg");
            _animes.Setup(r => r.AddAsync(It.IsAny<Anime>())).ThrowsAsync(new InvalidOperationException("db"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(new AnimeCreateDto { Title = "A" }, NewUpload()));

            _files.Verify(f => f.DeletePosterAsync("/uploads/posters/new.jpg"), Times.Once);
        }

        [Fact]
        public async Task Update_UnknownId_ThrowsNotFound()
        {
            _animes.Setup(r => r.GetForUpdateAsync(7)).ReturnsAsync((Anime?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(UpdateDto(), null));
        }

        [Fact]
        public async Task Update_CopiesFields_AndTrimsTitle()
        {
            var anime = ExistingAnime();
            var dto = UpdateDto();
            dto.Title = "  New title  ";
            dto.Description = "Desc";
            dto.ReleaseYear = 2010;
            dto.Status = AnimeStatus.Ongoing;

            await _service.UpdateAsync(dto, null);

            Assert.Equal("New title", anime.Title);
            Assert.Equal("Desc", anime.Description);
            Assert.Equal(2010, anime.ReleaseYear);
            Assert.Equal(AnimeStatus.Ongoing, anime.Status);
            _animes.Verify(r => r.UpdateAsync(anime), Times.Once);
        }

        [Fact]
        public async Task Update_ReplacesGenres()
        {
            var anime = ExistingAnime();
            var oldGenre = TestData.CreateGenre("Old");
            oldGenre.Id = 1;
            anime.Genres.Add(oldGenre);

            var newGenre = TestData.CreateGenre("New");
            newGenre.Id = 2;
            _genres.Setup(g => g.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Genre> { newGenre });

            var dto = UpdateDto();
            dto.SelectedGenreIds = [2];

            await _service.UpdateAsync(dto, null);

            Assert.Same(newGenre, Assert.Single(anime.Genres));
        }

        [Fact]
        public async Task Update_WithoutNewPoster_KeepsCurrentPoster()
        {
            var anime = ExistingAnime(posterUrl: "/uploads/posters/old.jpg");

            await _service.UpdateAsync(UpdateDto(), null);

            Assert.Equal("/uploads/posters/old.jpg", anime.PosterUrl);
            _files.Verify(f => f.DeletePosterAsync(It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task Update_NewUploadedPoster_ReplacesOld_AndDeletesOldFileAfterSaving()
        {
            var anime = ExistingAnime(posterUrl: "/uploads/posters/old.jpg");
            _files.Setup(f => f.SavePosterAsync(It.IsAny<FileUploadDto>())).ReturnsAsync("/uploads/posters/new.jpg");

            var calls = new List<string>();
            _animes.Setup(r => r.UpdateAsync(It.IsAny<Anime>()))
                .Callback(() => calls.Add("update"))
                .Returns(Task.CompletedTask);
            _files.Setup(f => f.DeletePosterAsync("/uploads/posters/old.jpg"))
                .Callback(() => calls.Add("deleteOld"))
                .Returns(Task.CompletedTask);

            await _service.UpdateAsync(UpdateDto(), NewUpload());

            Assert.Equal("/uploads/posters/new.jpg", anime.PosterUrl);

            Assert.Equal(new[] { "update", "deleteOld" }, calls);
        }

        [Fact]
        public async Task Update_ExternalPosterUrl_ReplacesLocalPoster_AndDeletesOldFile()
        {
            var anime = ExistingAnime(posterUrl: "/uploads/posters/old.jpg");
            var dto = UpdateDto();
            dto.PosterUrl = "https://example.com/new.jpg";

            await _service.UpdateAsync(dto, null);

            Assert.Equal("https://example.com/new.jpg", anime.PosterUrl);
            _files.Verify(f => f.DeletePosterAsync("/uploads/posters/old.jpg"), Times.Once);
        }

        [Fact]
        public async Task Update_WhenDatabaseFails_DeletesNewFile_AndKeepsOldOne()
        {
            ExistingAnime(posterUrl: "/uploads/posters/old.jpg");
            _files.Setup(f => f.SavePosterAsync(It.IsAny<FileUploadDto>())).ReturnsAsync("/uploads/posters/new.jpg");
            _animes.Setup(r => r.UpdateAsync(It.IsAny<Anime>())).ThrowsAsync(new InvalidOperationException("db"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(UpdateDto(), NewUpload()));

            _files.Verify(f => f.DeletePosterAsync("/uploads/posters/new.jpg"), Times.Once);
            _files.Verify(f => f.DeletePosterAsync("/uploads/posters/old.jpg"), Times.Never);
        }

        [Fact]
        public async Task Update_InvalidPlayerUrl_Throws_AndDoesNotTouchEntityOrFiles()
        {
            var anime = ExistingAnime(playerUrl: "https://example.com/old");
            var dto = UpdateDto();
            dto.PlayerUrl = "http://insecure.example.com";

            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(dto, NewUpload()));

            Assert.Equal("Old", anime.Title);
            Assert.Equal("https://example.com/old", anime.PlayerUrl);
            _files.Verify(f => f.SavePosterAsync(It.IsAny<FileUploadDto>()), Times.Never);
            _animes.Verify(r => r.UpdateAsync(It.IsAny<Anime>()), Times.Never);
        }

        [Fact]
        public async Task Update_EmptyPlayerUrl_RemovesTheLink()
        {
            var anime = ExistingAnime(playerUrl: "https://example.com/old");
            var dto = UpdateDto();
            dto.PlayerUrl = "";

            await _service.UpdateAsync(dto, null);

            Assert.Null(anime.PlayerUrl);
        }

        [Fact]
        public async Task Update_NewPlayerUrl_IsNormalized()
        {
            var anime = ExistingAnime();
            var dto = UpdateDto();
            dto.PlayerUrl = "//kodik.info/video/2/x/720p";

            await _service.UpdateAsync(dto, null);

            Assert.Equal("https://kodik.info/video/2/x/720p", anime.PlayerUrl);
        }

        [Fact]
        public async Task Delete_UnknownId_ThrowsNotFound()
        {
            _animes.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Anime?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(7));
        }

        [Fact]
        public async Task Delete_RemovesFromDatabaseFirst_ThenDeletesPosterFile()
        {
            var anime = new Anime { Id = 7, Title = "A", PosterUrl = "/uploads/posters/old.jpg" };
            _animes.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(anime);

            var calls = new List<string>();
            _animes.Setup(r => r.DeleteAsync(anime))
                .Callback(() => calls.Add("deleteDb"))
                .Returns(Task.CompletedTask);
            _files.Setup(f => f.DeletePosterAsync("/uploads/posters/old.jpg"))
                .Callback(() => calls.Add("deleteFile"))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(7);

            Assert.Equal(new[] { "deleteDb", "deleteFile" }, calls);
        }

        [Fact]
        public async Task Delete_WhenDatabaseFails_DoesNotDeleteFile()
        {
            var anime = new Anime { Id = 7, Title = "A", PosterUrl = "/uploads/posters/old.jpg" };
            _animes.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(anime);
            _animes.Setup(r => r.DeleteAsync(anime)).ThrowsAsync(new InvalidOperationException("db"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(7));

            _files.Verify(f => f.DeletePosterAsync(It.IsAny<string?>()), Times.Never);
        }
    }
}
