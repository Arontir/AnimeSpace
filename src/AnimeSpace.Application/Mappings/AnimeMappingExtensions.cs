using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    public static class AnimeMappingExtensions
    {
        public static AnimeCardDto ToCardDto(this Anime anime) => new AnimeCardDto()
        {
            Id = anime.Id,
            Title = anime.Title,
            PosterUrl = anime.PosterUrl,
            ReleaseYear = anime.ReleaseYear,
            Status = anime.Status,
            AverageRating = anime.AverageRating,
            RatingsCount = anime.RatingsCount,
            GenreNames = anime.Genres.Select(g => g.Name).OrderBy(n => n).ToList()
        };

        public static AnimeDetailsDto ToDetailsDto(this Anime anime) => new AnimeDetailsDto()
        {
            Id = anime.Id,
            Title = anime.Title,
            Description = anime.Description,
            ReleaseYear = anime.ReleaseYear,
            PosterUrl = anime.PosterUrl,
            Status = anime.Status,
            AverageRating = anime.AverageRating,
            RatingsCount = anime.RatingsCount,
            PlayerUrl = anime.PlayerUrl,
            Genres = anime.Genres.Select(g => g.ToDto()).OrderBy(g => g).ToList()
        };

        public static AnimeUpdateDto ToUpdateDto(this Anime anime)
        {
            bool isExternalPoster = !string.IsNullOrWhiteSpace(anime.PosterUrl) &&
                (anime.PosterUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 anime.PosterUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

            return new AnimeUpdateDto()
            {
                Id = anime.Id,
                CurrentPosterUrl = anime.PosterUrl,
                Title = anime.Title,
                Description = anime.Description,
                ReleaseYear = anime.ReleaseYear,
                PosterUrl = isExternalPoster ? anime.PosterUrl : null,
                PlayerUrl = anime.PlayerUrl,
                Status = anime.Status,
                SelectedGenreIds = anime.Genres.Select(g => g.Id).ToList()
            };
        }

        public static Anime ToEntity(this AnimeCreateDto dto) => new Anime()
        {
            Title = dto.Title,
            Description = dto.Description,
            ReleaseYear = dto.ReleaseYear,
            PosterUrl = dto.PosterUrl,
            PlayerUrl = dto.PlayerUrl,
            Status = dto.Status,
        };

        public static void ApplyTo(this AnimeUpdateDto dto, Anime existing)
        {
            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.ReleaseYear = dto.ReleaseYear;
            existing.Status = dto.Status;
            existing.PlayerUrl = dto.PlayerUrl;
        }
    }
}
