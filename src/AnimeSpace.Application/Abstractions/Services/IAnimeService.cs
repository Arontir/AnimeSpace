using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    /// <summary>
    /// Бізнес-логіка роботи з тайтлами: каталог, перегляд, створення, редагування, видалення.
    /// </summary>
    public interface IAnimeService
    {
        /// <summary>
        /// Повертає повну інформацію про тайтл для сторінки перегляду.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        public Task<AnimeDetailsDto> GetDetailsAsync(int id);

        /// <summary>
        /// Повертає дані для форми редагування. Поле PosterUrl заповнене лише для зовнішнього постера;
        /// поточний постер (будь-який) лежить у CurrentPosterUrl.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        public Task<AnimeUpdateDto> GetForEditAsync(int id);

        /// <summary>
        /// Повертає сторінку каталогу. Перед запитом до бази нормалізує копію фільтра
        /// (Page ≥ 1, PageSize від 1 до 48, SearchQuery без зайвих пробілів); вхідний об'єкт не змінюється.
        /// </summary>
        /// <param name="filter">Параметри пошуку, фільтрації та сортування.</param>
        public Task<PagedResult<AnimeCardDto>> GetCatalogAsync(AnimeFilterDto filter);

        /// <summary>
        /// Повертає останні додані тайтли. Кількість обмежується діапазоном від 1 до 20.
        /// </summary>
        /// <param name="count">Кількість тайтлів.</param>
        public Task<IReadOnlyList<AnimeCardDto>> GetLatestAsync(int count);

        /// <summary>
        /// Повертає тайтли з найвищим рейтингом. Кількість обмежується діапазоном від 1 до 20.
        /// </summary>
        /// <param name="count">Кількість тайтлів.</param>
        public Task<IReadOnlyList<AnimeCardDto>> GetTopRatedAsync(int count);

        /// <summary>
        /// Створює тайтл. Перевіряє назву та посилання на плеєр (порожнє → <c>null</c>; "//host/..." → "https://host/...";
        /// дозволено лише https), завантажує жанри. Постер: завантажений файл має пріоритет над зовнішнім посиланням.
        /// Якщо збереження в базі не вдалося, збережений файл постера видаляється.
        /// </summary>
        /// <param name="dto">Дані форми.</param>
        /// <param name="poster">Завантажений файл постера або <c>null</c>.</param>
        /// <returns>Ідентифікатор нового тайтлу.</returns>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">
        /// Порожня назва, некоректне посилання на плеєр, неіснуючий жанр або неприпустимий файл.
        /// </exception>
        public Task<int> CreateAsync(AnimeCreateDto dto, FileUploadDto? poster);

        /// <summary>
        /// Оновлює тайтл. Постер змінюється лише якщо надійшов файл або непорожнє зовнішнє посилання, інакше лишається поточний.
        /// Посилання на плеєр замінюється значенням з форми (порожнє поле = прибрати посилання).
        /// Новий постер зберігається до оновлення бази, старий видаляється після успішного збереження.
        /// </summary>
        /// <param name="dto">Дані форми разом з ідентифікатором.</param>
        /// <param name="poster">Новий файл постера або <c>null</c>.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        /// <exception cref="AnimeSpace.Application.Exceptions.ValidationException">Порушено правила перевірки.</exception>
        public Task UpdateAsync(AnimeUpdateDto dto, FileUploadDto? poster);

        /// <summary>
        /// Видаляє тайтл із бази, а потім його локальний постер (зовнішні посилання не чіпає).
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        /// <exception cref="AnimeSpace.Application.Exceptions.NotFoundException">Тайтл не знайдено.</exception>
        public Task DeleteAsync(int id);
    }
}
