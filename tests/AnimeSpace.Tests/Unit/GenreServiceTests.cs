using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Services;
using AnimeSpace.Domain.Entities;
using Moq;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Юніт-тести GenreService: унікальність назви, обрізання пробілів, перейменування.
    /// </summary>
    public class GenreServiceTests
    {
        private readonly Mock<IGenreRepository> _genres = new();
        private readonly GenreService _service;

        public GenreServiceTests()
        {
            _service = new GenreService(_genres.Object);
        }

        [Fact]
        public async Task GetAll_MapsGenresToDtos()
        {
            _genres.Setup(g => g.GetAllAsync()).ReturnsAsync(new List<Genre>
            {
                new() { Id = 1, Name = "Action" },
                new() { Id = 2, Name = "Drama" }
            });

            var result = await _service.GetAllAsync();

            Assert.Equal(new[] { "Action", "Drama" }, result.Select(g => g.Name));
        }

        [Fact]
        public async Task GetById_Unknown_ThrowsNotFound()
        {
            _genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync((Genre?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(5));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_BlankName_Throws(string name)
        {
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateAsync(new GenreCreateDto { Name = name }));

            _genres.Verify(g => g.AddAsync(It.IsAny<Genre>()), Times.Never);
        }

        [Fact]
        public async Task Create_TooLongName_Throws()
        {
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateAsync(new GenreCreateDto { Name = new string('a', 101) }));
        }

        [Fact]
        public async Task Create_DuplicateName_Throws()
        {
            _genres.Setup(g => g.NameExistsAsync("Action", It.IsAny<int?>())).ReturnsAsync(true);

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateAsync(new GenreCreateDto { Name = "Action" }));

            _genres.Verify(g => g.AddAsync(It.IsAny<Genre>()), Times.Never);
        }

        [Fact]
        public async Task Create_TrimsName_AndReturnsNewId()
        {
            _genres.Setup(g => g.NameExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            Genre? added = null;
            _genres.Setup(g => g.AddAsync(It.IsAny<Genre>()))
                .Callback<Genre>(g =>
                {
                    g.Id = 9;
                    added = g;
                })
                .Returns(Task.CompletedTask);

            var id = await _service.CreateAsync(new GenreCreateDto { Name = "  Action  " });

            Assert.Equal(9, id);
            Assert.Equal("Action", added!.Name);
        }

        [Fact]
        public async Task Update_UnknownGenre_ThrowsNotFound()
        {
            _genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync((Genre?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(5, new GenreCreateDto { Name = "X" }));
        }

        [Fact]
        public async Task Update_ChecksUniquenessExcludingTheGenreItself()
        {
            var genre = new Genre { Id = 5, Name = "Old" };
            _genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync(genre);
            _genres.Setup(g => g.NameExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);

            await _service.UpdateAsync(5, new GenreCreateDto { Name = "  New  " });

            Assert.Equal("New", genre.Name);
            _genres.Verify(g => g.NameExistsAsync("New", 5), Times.Once);
            _genres.Verify(g => g.UpdateAsync(genre), Times.Once);
        }

        [Fact]
        public async Task Update_NameTakenByAnotherGenre_Throws_AndKeepsOldName()
        {
            var genre = new Genre { Id = 5, Name = "Old" };
            _genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync(genre);
            _genres.Setup(g => g.NameExistsAsync("Action", 5)).ReturnsAsync(true);

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateAsync(5, new GenreCreateDto { Name = "Action" }));

            Assert.Equal("Old", genre.Name);
            _genres.Verify(g => g.UpdateAsync(It.IsAny<Genre>()), Times.Never);
        }

        [Fact]
        public async Task Delete_UnknownGenre_ThrowsNotFound()
        {
            _genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync((Genre?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(5));
        }

        [Fact]
        public async Task Delete_ExistingGenre_IsRemoved()
        {
            var genre = new Genre { Id = 5, Name = "Old" };
            _genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync(genre);

            await _service.DeleteAsync(5);

            _genres.Verify(g => g.DeleteAsync(genre), Times.Once);
        }
    }
}
