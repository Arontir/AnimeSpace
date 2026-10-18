using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Services;
using AnimeSpace.Domain.Entities;
using Moq;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Юніт-тести CommentService: перевірки тексту, права на видалення, повторне читання після додавання.
    /// </summary>
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _comments = new();
        private readonly Mock<IAnimeRepository> _animes = new();
        private readonly CommentService _service;

        public CommentServiceTests()
        {
            _animes.Setup(a => a.ExistsAsync(1)).ReturnsAsync(true);
            _service = new CommentService(_comments.Object, _animes.Object);
        }

        /// <summary>
        /// Налаштовує репозиторій так, як поводиться база: AddAsync присвоює Id,
        /// а GetByIdAsync повертає коментар уже із завантаженим автором.
        /// </summary>
        private void SetupSavingWithAuthor(string authorUserName = "neo@mail.com")
        {
            Comment? added = null;

            _comments.Setup(c => c.AddAsync(It.IsAny<Comment>()))
                .Callback<Comment>(c =>
                {
                    c.Id = 5;
                    added = c;
                })
                .Returns(Task.CompletedTask);

            _comments.Setup(c => c.GetByIdAsync(5)).ReturnsAsync(() => new Comment
            {
                Id = 5,
                Text = added!.Text,
                UserId = added.UserId,
                AnimeId = added.AnimeId,
                CreatedAt = added.CreatedAt,
                User = new ApplicationUser { UserName = authorUserName }
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Add_EmptyText_Throws_AndWritesNothing(string text)
        {
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddAsync("u1", 1, text));

            _comments.Verify(c => c.AddAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Add_TooLongText_Throws()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddAsync("u1", 1, new string('a', 2001)));
        }

        [Fact]
        public async Task Add_TextOfMaxLength_IsAccepted()
        {
            SetupSavingWithAuthor();

            var dto = await _service.AddAsync("u1", 1, new string('a', 2000));

            Assert.Equal(2000, dto.Text.Length);
        }

        [Fact]
        public async Task Add_UnknownAnime_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.AddAsync("u1", 99, "hello"));

            _comments.Verify(c => c.AddAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Add_StoresTrimmedTextAsTyped_WithoutHtmlRewriting()
        {
            SetupSavingWithAuthor();

            var dto = await _service.AddAsync("u1", 1, "  <b>hi</b>  ");

            Assert.Equal("<b>hi</b>", dto.Text);
        }

        [Fact]
        public async Task Add_RereadsCommentWithAuthor_SoResultHasDisplayName()
        {
            SetupSavingWithAuthor("neo@mail.com");

            var dto = await _service.AddAsync("u1", 1, "hello");

            Assert.Equal(5, dto.Id);
            Assert.Equal("neo", dto.UserName);
            _comments.Verify(c => c.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task Delete_UnknownComment_ThrowsNotFound()
        {
            _comments.Setup(c => c.GetByIdAsync(5)).ReturnsAsync((Comment?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(5, "u1", isAdmin: false));
        }

        [Fact]
        public async Task Delete_OwnComment_IsAllowed()
        {
            var comment = new Comment { Id = 5, Text = "x", UserId = "u1", AnimeId = 1 };
            _comments.Setup(c => c.GetByIdAsync(5)).ReturnsAsync(comment);

            await _service.DeleteAsync(5, "u1", isAdmin: false);

            _comments.Verify(c => c.DeleteAsync(comment), Times.Once);
        }

        [Fact]
        public async Task Delete_SomeoneElsesComment_ByRegularUser_ThrowsForbidden()
        {
            var comment = new Comment { Id = 5, Text = "x", UserId = "u1", AnimeId = 1 };
            _comments.Setup(c => c.GetByIdAsync(5)).ReturnsAsync(comment);

            await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(5, "u2", isAdmin: false));

            _comments.Verify(c => c.DeleteAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Delete_SomeoneElsesComment_ByAdmin_IsAllowed()
        {
            var comment = new Comment { Id = 5, Text = "x", UserId = "u1", AnimeId = 1 };
            _comments.Setup(c => c.GetByIdAsync(5)).ReturnsAsync(comment);

            await _service.DeleteAsync(5, "admin-id", isAdmin: true);

            _comments.Verify(c => c.DeleteAsync(comment), Times.Once);
        }
    }
}
