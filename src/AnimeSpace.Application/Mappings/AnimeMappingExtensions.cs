using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    /// <summary>
    /// Перетворення між сутністю <c>Anime</c> та її DTO.
    /// </summary>
    public static class AnimeMappingExtensions
    {
        /// <summary>
        /// Створює картку для каталогу. Назви жанрів відсортовано за алфавітом (жанри мають бути завантажені через Include).
        /// </summary>
        /// <param name="anime">Тайтл.</param>
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

        /// <summary>
        /// Створює повну інформацію для сторінки перегляду. Жанри відсортовано за назвою.
        /// </summary>
        /// <param name="anime">Тайтл.</param>
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

        /// <summary>
        /// Створює дані для форми редагування. CurrentPosterUrl отримує поточний постер,
        /// а PosterUrl — лише якщо постер зовнішній (http/https).
        /// </summary>
        /// <param name="anime">Тайтл із завантаженими жанрами.</param>
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

        /// <summary>
        /// Створює нову сутність із даних форми. Жанри не заповнюються — це робить сервіс.
        /// PlayerUrl копіюється як є, тому сервіс має нормалізувати його до виклику.
        /// </summary>
        /// <param name="dto">Дані форми створення.</param>
        public static Anime ToEntity(this AnimeCreateDto dto) => new Anime()
        {
            Title = dto.Title,
            Description = dto.Description,
            ReleaseYear = dto.ReleaseYear,
            PosterUrl = dto.PosterUrl,
            PlayerUrl = dto.PlayerUrl,
            Status = dto.Status,
        };

        /// <summary>
        /// Переносить дані форми в уже завантажену сутність. PosterUrl і Genres не змінюються:
        /// ними керує AnimeService. PlayerUrl копіюється як є (нормалізацію виконує сервіс до виклику).
        /// </summary>
        /// <param name="dto">Дані форми редагування.</param>
        /// <param name="existing">Тайтл, завантажений через GetForUpdateAsync.</param>
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
