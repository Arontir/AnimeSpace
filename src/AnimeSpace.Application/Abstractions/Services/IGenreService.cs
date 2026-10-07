using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    /// <summary>
    /// Бізнес-логіка керування жанрами.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Повертає всі жанри за алфавітом.
        /// </summary>
        public Task<IReadOnlyList<GenreDto>> GetAllAsync();

        /// <summary>
        /// Повертає жанр за ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Жанр не знайдено.</exception>
        public Task<GenreDto> GetByIdAsync(int id);

        /// <summary>
        /// Створює жанр: обрізає пробіли та перевіряє, що такої назви ще немає.
        /// </summary>
        /// <param name="dto">Дані форми.</param>
        /// <returns>Ідентифікатор нового жанру.</returns>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Жанр із такою назвою вже існує.</exception>
        public Task<int> CreateAsync(GenreCreateDto dto);

        /// <summary>
        /// Перейменовує жанр з тими самими перевірками, що й під час створення.
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        /// <param name="dto">Нова назва.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Жанр не знайдено.</exception>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Жанр із такою назвою вже існує.</exception>
        public Task UpdateAsync(int id, GenreCreateDto dto);

        /// <summary>
        /// Видаляє жанр. Зв'язки з тайтлами видаляються разом із ним.
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Жанр не знайдено.</exception>
        public Task DeleteAsync(int id);
    }
}
