using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Mappings;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Tests.Helpers;

namespace AnimeSpace.Tests.Unit
{
    /// <summary>
    /// Тести методів перетворення сутностей у DTO та назад.
    /// </summary>
    public class MappingTests
    {
        [Theory]
        [InlineData("user@mail.com", "user")]
        [InlineData("a.b-c@example.org", "a.b-c")]
        [InlineData("no-at-sign", "no-at-sign")]
        public void CommentToDto_UserName_IsPartBeforeAtSign(string userName, string expected)
        {
            var comment = new Comment
            {
                Id = 1, Text = "x", UserId = "u1", AnimeId = 1,
                User = new ApplicationUser { UserName = userName }
            };

            Assert.Equal(expected, comment.ToDto().UserName);
        }

        [Fact]
        public void CommentToDto_MapsAllFields()
        {
            var created = new DateTime(2025, 5, 1, 12, 0, 0, DateTimeKind.Utc);
            var comment = new Comment
            {
                Id = 3, Text = "hello", UserId = "u1", AnimeId = 1, CreatedAt = created,
                User = new ApplicationUser { UserName = "neo@mail.com" }
            };

            var dto = comment.ToDto();

            Assert.Equal(3, dto.Id);
            Assert.Equal("hello", dto.Text);
            Assert.Equal("u1", dto.UserId);
            Assert.Equal(created, dto.CreatedAt);
        }

        [Fact]
        public void AnimeToCardDto_GenreNames_AreSortedAlphabetically()
        {
            var anime = TestData.CreateAnime("A", 2020, AnimeStatus.Finished,
                TestData.CreateGenre("Drama"), TestData.CreateGenre("Action"), TestData.CreateGenre("Comedy"));

            var card = anime.ToCardDto();

            Assert.Equal(new[] { "Action", "Comedy", "Drama" }, card.GenreNames);
        }

        [Fact]
        public void AnimeToCardDto_MapsMainFields()
        {
            var anime = TestData.CreateAnime("Bebop", 1998, AnimeStatus.Finished);
            anime.Id = 4;
            anime.PosterUrl = "https://example.com/p.jpg";
            anime.AverageRating = 8.5;
            anime.RatingsCount = 12;

            var card = anime.ToCardDto();

            Assert.Equal(4, card.Id);
            Assert.Equal("Bebop", card.Title);
            Assert.Equal(1998, card.ReleaseYear);
            Assert.Equal(AnimeStatus.Finished, card.Status);
            Assert.Equal("https://example.com/p.jpg", card.PosterUrl);
            Assert.Equal(8.5, card.AverageRating);
            Assert.Equal(12, card.RatingsCount);
        }

        [Fact]
        public void AnimeToDetailsDto_GenresAreSortedByName_AndPlayerUrlIsCopied()
        {
            var anime = TestData.CreateAnime("A", 2020, AnimeStatus.Ongoing,
                TestData.CreateGenre("Drama"), TestData.CreateGenre("Action"));
            anime.PlayerUrl = "https://example.com/player";

            var dto = anime.ToDetailsDto();

            Assert.Equal(new[] { "Action", "Drama" }, dto.Genres.Select(g => g.Name));
            Assert.Equal("https://example.com/player", dto.PlayerUrl);
        }

        [Fact]
        public void AnimeToUpdateDto_ExternalPoster_IsAvailableInBothFields()
        {
            var anime = TestData.CreateAnime("A");
            anime.PosterUrl = "https://example.com/p.jpg";

            var dto = anime.ToUpdateDto();

            Assert.Equal("https://example.com/p.jpg", dto.PosterUrl);
            Assert.Equal("https://example.com/p.jpg", dto.CurrentPosterUrl);
        }

        [Fact]
        public void AnimeToUpdateDto_LocalPoster_IsOnlyInCurrentPosterUrl()
        {
            var anime = TestData.CreateAnime("A");
            anime.PosterUrl = "/uploads/posters/x.jpg";

            var dto = anime.ToUpdateDto();

            Assert.Null(dto.PosterUrl);
            Assert.Equal("/uploads/posters/x.jpg", dto.CurrentPosterUrl);
        }

        [Fact]
        public void AnimeToUpdateDto_NoPoster_BothFieldsAreNull()
        {
            var dto = TestData.CreateAnime("A").ToUpdateDto();

            Assert.Null(dto.PosterUrl);
            Assert.Null(dto.CurrentPosterUrl);
        }

        [Fact]
        public void AnimeToUpdateDto_SelectedGenreIds_ComeFromGenres()
        {
            var action = TestData.CreateGenre("Action");
            action.Id = 3;
            var drama = TestData.CreateGenre("Drama");
            drama.Id = 8;
            var anime = TestData.CreateAnime("A", 2020, AnimeStatus.Finished, action, drama);
            anime.Id = 5;

            var dto = anime.ToUpdateDto();

            Assert.Equal(5, dto.Id);
            Assert.Equal(new[] { 3, 8 }.Order(), dto.SelectedGenreIds.Order());
        }

        [Fact]
        public void AnimeCreateDtoToEntity_CopiesFields_ButNotGenres()
        {
            var dto = new AnimeCreateDto
            {
                Title = "A", Description = "D", ReleaseYear = 2001, PosterUrl = "https://example.com/p.jpg",
                PlayerUrl = "https://example.com/player", Status = AnimeStatus.Ongoing, SelectedGenreIds = [1, 2]
            };

            var anime = dto.ToEntity();

            Assert.Equal("A", anime.Title);
            Assert.Equal("D", anime.Description);
            Assert.Equal(2001, anime.ReleaseYear);
            Assert.Equal("https://example.com/p.jpg", anime.PosterUrl);
            Assert.Equal("https://example.com/player", anime.PlayerUrl);
            Assert.Equal(AnimeStatus.Ongoing, anime.Status);
            Assert.Empty(anime.Genres);
        }

        [Fact]
        public void AnimeUpdateDtoApplyTo_DoesNotTouchPosterOrGenres()
        {
            var genre = TestData.CreateGenre("Keep");
            var existing = TestData.CreateAnime("Old", 1990, AnimeStatus.Finished, genre);
            existing.PosterUrl = "/uploads/posters/old.jpg";

            var dto = new AnimeUpdateDto
            {
                Id = 1, Title = "New", Description = "D", ReleaseYear = 2020, Status = AnimeStatus.Ongoing,
                PlayerUrl = "https://example.com/player", PosterUrl = "https://example.com/other.jpg",
                SelectedGenreIds = [99]
            };

            dto.ApplyTo(existing);

            Assert.Equal("New", existing.Title);
            Assert.Equal("D", existing.Description);
            Assert.Equal(2020, existing.ReleaseYear);
            Assert.Equal(AnimeStatus.Ongoing, existing.Status);
            Assert.Equal("https://example.com/player", existing.PlayerUrl);

            Assert.Equal("/uploads/posters/old.jpg", existing.PosterUrl);
            Assert.Same(genre, Assert.Single(existing.Genres));
        }

        [Fact]
        public void GenreMapping_RoundTrip()
        {
            var dto = new Genre { Id = 2, Name = "Drama" }.ToDto();
            var entity = new GenreCreateDto { Name = "Comedy" }.ToEntity();

            Assert.Equal(2, dto.Id);
            Assert.Equal("Drama", dto.Name);
            Assert.Equal("Comedy", entity.Name);
        }

        [Fact]
        public void BookmarkToDto_TakesTitleAndPosterFromAnime()
        {
            var updated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var bookmark = new Bookmark
            {
                UserId = "u1", AnimeId = 4, Status = WatchStatus.Watching, UpdatedAt = updated,
                Anime = new Anime { Title = "Bebop", PosterUrl = "https://example.com/p.jpg" }
            };

            var dto = bookmark.ToDto();

            Assert.Equal(4, dto.AnimeId);
            Assert.Equal("Bebop", dto.AnimeTitle);
            Assert.Equal("https://example.com/p.jpg", dto.AnimePosterUrl);
            Assert.Equal(WatchStatus.Watching, dto.Status);
            Assert.Equal(updated, dto.UpdatedAt);
        }
    }
}
