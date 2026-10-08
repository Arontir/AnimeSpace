using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Mappings;

namespace AnimeSpace.Application.Services
{
    /// <summary>
    /// Бізнес-логіка керування жанрами (панель адміністратора).
    /// Назва жанру унікальна: перевірка однакова для створення та перейменування.
    /// </summary>
    /// <param name="genres">Репозиторій жанрів.</param>
    public class GenreService(IGenreRepository genres) : IGenreService
    {
        private const int MaxNameLength = 100;

        /// <inheritdoc/>
        public async Task<IReadOnlyList<GenreDto>> GetAllAsync()
        {
            var list = await genres.GetAllAsync();
            return list.Select(g => g.ToDto()).ToList();
        }

        /// <inheritdoc/>
        public async Task<GenreDto> GetByIdAsync(int id)
        {
            var genre = await genres.GetByIdAsync(id) ?? throw new NotFoundException("Жанр не знайдено.");

            return genre.ToDto();
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(GenreCreateDto dto)
        {
            var name = await ValidateNameAsync(dto.Name, excludeGenreId: null);

            var genre = dto.ToEntity();
            genre.Name = name;

            await genres.AddAsync(genre);

            return genre.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, GenreCreateDto dto)
        {
            var genre = await genres.GetByIdAsync(id) ?? throw new NotFoundException("Жанр не знайдено.");

            var name = await ValidateNameAsync(dto.Name, excludeGenreId: id);

            genre.Name = name;

            await genres.UpdateAsync(genre);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var genre = await genres.GetByIdAsync(id)
                ?? throw new NotFoundException("Жанр не знайдено.");

            await genres.DeleteAsync(genre);
        }

        /// <summary>
        /// Перевіряє назву жанру й повертає її без зайвих пробілів.
        /// </summary>
        /// <param name="name">Назва з форми.</param>
        /// <param name="excludeGenreId">Ідентифікатор жанру, який не враховується під час перевірки унікальності (перейменування).</param>
        /// <exception cref="ValidationException">Назва порожня, задовга або вже існує.</exception>
        private async Task<string> ValidateNameAsync(string? name, int? excludeGenreId)
        {
            var trimmed = name?.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                throw new ValidationException("Вкажіть назву жанру.");
            }

            if (trimmed.Length > MaxNameLength)
            {
                throw new ValidationException($"Назва жанру має бути не довшою за {MaxNameLength} символів.");
            }

            if (await genres.NameExistsAsync(trimmed, excludeGenreId))
            {
                throw new ValidationException("Жанр із такою назвою вже існує.");
            }

            return trimmed;
        }
    }
}