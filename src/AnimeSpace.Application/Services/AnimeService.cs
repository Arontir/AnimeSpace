using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.Abstractions.Storage;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Mappings;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Services
{
    /// <summary>
    /// Бізнес-логіка тайтлів: каталог, перегляд, створення, редагування та видалення.
    /// Сервіс не знає про HTTP, IFormFile і EF: працює лише з DTO, сутностями та інтерфейсами.
    /// </summary>
    /// <param name="animes">Репозиторій тайтлів.</param>
    /// <param name="genres">Репозиторій жанрів.</param>
    /// <param name="files">Сховище файлів постерів.</param>
    public class AnimeService(IAnimeRepository animes, IGenreRepository genres, IFileService files) : IAnimeService
    {
        /// <summary>
        /// Максимальний розмір сторінки каталогу.
        /// </summary>
        private const int MaxPageSize = 48;

        /// <summary>
        /// Максимальна кількість елементів у коротких списках (новинки, топ).
        /// </summary>
        private const int MaxListCount = 20;

        private const int MaxTitleLength = 200;
        private const int MaxPosterUrlLength = 500;
        private const int MaxPlayerUrlLength = 1000;

        /// <inheritdoc/>
        public async Task<AnimeDetailsDto> GetDetailsAsync(int id)
        {
            var anime = await animes.GetDetailsAsync(id) ?? throw new NotFoundException("Аніме не знайдено.");

            return anime.ToDetailsDto();
        }

        /// <inheritdoc/>
        public async Task<AnimeUpdateDto> GetForEditAsync(int id)
        {
            var anime = await animes.GetDetailsAsync(id) ?? throw new NotFoundException("Аніме не знайдено.");

            return anime.ToUpdateDto();
        }

        /// <inheritdoc/>
        public async Task<PagedResult<AnimeCardDto>> GetCatalogAsync(AnimeFilterDto filter)
        {
            var normalized = new AnimeFilterDto
            {
                SearchQuery = string.IsNullOrWhiteSpace(filter.SearchQuery) ? null : filter.SearchQuery.Trim(),
                GenreId = filter.GenreId,
                ReleaseYear = filter.ReleaseYear,
                Status = filter.Status is { } status && Enum.IsDefined(status) ? status : null,
                SortBy = Enum.IsDefined(filter.SortBy) ? filter.SortBy : AnimeSortBy.Newest,
                Page = Math.Max(1, filter.Page),
                PageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize)
            };

            var page = await animes.GetFilteredAsync(normalized);

            return new PagedResult<AnimeCardDto>
            {
                Items = page.Items.Select(a => a.ToCardDto()).ToList(),
                TotalCount = page.TotalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            };
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<AnimeCardDto>> GetLatestAsync(int count)
        {
            var list = await animes.GetLatestAsync(Math.Clamp(count, 1, MaxListCount));
            return list.Select(a => a.ToCardDto()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<AnimeCardDto>> GetTopRatedAsync(int count)
        {
            var list = await animes.GetTopRatedAsync(Math.Clamp(count, 1, MaxListCount));
            return list.Select(a => a.ToCardDto()).ToList();
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(AnimeCreateDto dto, FileUploadDto? poster)
        {
            var title = ValidateTitle(dto.Title);
            var playerUrl = NormalizePlayerUrl(dto.PlayerUrl);
            var externalPoster = NormalizePosterUrl(dto.PosterUrl);
            var selectedGenres = await LoadGenresAsync(dto.SelectedGenreIds);

            var anime = dto.ToEntity();
            anime.Title = title;
            anime.PlayerUrl = playerUrl;
            anime.PosterUrl = externalPoster;

            foreach (var genre in selectedGenres)
            {
                anime.Genres.Add(genre);
            }

            string? savedPoster = null;
            if (poster is not null)
            {
                savedPoster = await files.SavePosterAsync(poster);
                anime.PosterUrl = savedPoster;
            }

            try
            {
                await animes.AddAsync(anime);
            }
            catch
            {
                if (savedPoster is not null)
                {
                    await files.DeletePosterAsync(savedPoster);
                }

                throw;
            }

            return anime.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(AnimeUpdateDto dto, FileUploadDto? poster)
        {
            var anime = await animes.GetForUpdateAsync(dto.Id) ?? throw new NotFoundException("Аніме не знайдено.");

            var oldPoster = anime.PosterUrl;

            var title = ValidateTitle(dto.Title);
            var playerUrl = NormalizePlayerUrl(dto.PlayerUrl);
            var externalPoster = NormalizePosterUrl(dto.PosterUrl);
            var selectedGenres = await LoadGenresAsync(dto.SelectedGenreIds);

            string? newPoster = null;
            if (poster is not null)
            {
                newPoster = await files.SavePosterAsync(poster);
            }

            dto.ApplyTo(anime);
            anime.Title = title;
            anime.PlayerUrl = playerUrl;

            anime.Genres.Clear();
            foreach (var genre in selectedGenres)
            {
                anime.Genres.Add(genre);
            }

            if (newPoster is not null)
            {
                anime.PosterUrl = newPoster;
            }
            else if (externalPoster is not null)
            {
                anime.PosterUrl = externalPoster;
            }

            try
            {
                await animes.UpdateAsync(anime);
            }
            catch
            {
                if (newPoster is not null)
                {
                    await files.DeletePosterAsync(newPoster);
                }

                throw;
            }

            if (anime.PosterUrl != oldPoster)
            {
                await files.DeletePosterAsync(oldPoster);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var anime = await animes.GetByIdAsync(id) ?? throw new NotFoundException("Аніме не знайдено.");

            var poster = anime.PosterUrl;

            await animes.DeleteAsync(anime);

            await files.DeletePosterAsync(poster);
        }


        /// <summary>Перевіряє назву й повертає її без зайвих пробілів.</summary>
        /// <param name="title">Назва з форми.</param>
        /// <exception cref="ValidationException">Назва порожня або задовга.</exception>
        private static string ValidateTitle(string? title)
        {
            var trimmed = title?.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                throw new ValidationException("Вкажіть назву.");
            }

            if (trimmed.Length > MaxTitleLength)
            {
                throw new ValidationException($"Назва має бути не довшою за {MaxTitleLength} символів.");
            }

            return trimmed;
        }

        /// <summary>
        /// Приводить посилання на плеєр до вигляду, придатного для iframe:
        /// порожнє → <c>null</c>; "//host/..." → "https://host/..."; дозволено лише https.
        /// Це відсікає javascript:, http:, ftp: та інше.
        /// </summary>
        /// <param name="url">Посилання з форми.</param>
        /// <returns>Нормалізоване посилання або <c>null</c>, якщо поле порожнє.</returns>
        /// <exception cref="ValidationException">Посилання некоректне, не https або задовге.</exception>
        private static string? NormalizePlayerUrl(string? url)
        {
            var value = url?.Trim();

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.StartsWith("//", StringComparison.Ordinal))
            {
                value = "https:" + value;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ValidationException("Посилання на плеєр має бути коректною адресою https.");
            }

            var result = uri.AbsoluteUri;

            if (result.Length > MaxPlayerUrlLength)
            {
                throw new ValidationException($"Посилання на плеєр має бути не довшим за {MaxPlayerUrlLength} символів.");
            }

            return result;
        }

        /// <summary>
        /// Перевіряє зовнішнє посилання на постер: порожнє → <c>null</c>, інакше лише http або https.
        /// Завдяки цьому в PosterUrl може потрапити або зовнішня адреса, або локальний шлях, який створив сам сервіс файлів
        /// (тож видалення файлів ніколи не торкнеться чужих шляхів).
        /// </summary>
        /// <param name="url">Посилання з форми.</param>
        /// <returns>Нормалізоване посилання або <c>null</c>, якщо поле порожнє.</returns>
        /// <exception cref="ValidationException">Посилання некоректне або задовге.</exception>
        private static string? NormalizePosterUrl(string? url)
        {
            var value = url?.Trim();

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException("Посилання на постер має починатися з http:// або https://.");
            }

            var result = uri.AbsoluteUri;

            if (result.Length > MaxPosterUrlLength)
            {
                throw new ValidationException($"Посилання на постер має бути не довшим за {MaxPosterUrlLength} символів.");
            }

            return result;
        }

        /// <summary>
        /// Завантажує жанри за ідентифікаторами. Повертає відстежувані сутності,
        /// які можна безпосередньо додавати в <c>anime.Genres</c>.
        /// </summary>
        /// <param name="ids">Ідентифікатори обраних жанрів (дублікати ігноруються).</param>
        /// <exception cref="ValidationException">Хоча б один жанр не існує.</exception>
        private async Task<IReadOnlyList<Genre>> LoadGenresAsync(IEnumerable<int> ids)
        {
            var distinctIds = ids.Distinct().ToList();

            if (distinctIds.Count == 0)
            {
                return [];
            }

            var found = await genres.GetByIdsAsync(distinctIds);

            if (found.Count != distinctIds.Count)
            {
                throw new ValidationException("Один або кілька обраних жанрів не існують.");
            }

            return found;
        }
    }
}