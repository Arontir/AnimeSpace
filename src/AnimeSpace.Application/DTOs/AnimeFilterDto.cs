using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Параметри пошуку, фільтрації, сортування та пагінації каталогу.
    /// </summary>
    public class AnimeFilterDto
    {
        /// <summary>
        /// Пошук за назвою.
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Фільтр за жанром.
        /// </summary>
        public int? GenreId { get; set; }

        /// <summary>
        /// Фільтр за роком виходу.
        /// </summary>
        public int? ReleaseYear { get; set; }

        /// <summary>
        /// Фільтр за статусом виходу.
        /// </summary>
        public AnimeStatus? Status { get; set; }

        /// <summary>
        /// Порядок сортування. За замовчуванням — спочатку нові.
        /// </summary>
        public AnimeSortBy SortBy { get; set; } = AnimeSortBy.Newest;

        /// <summary>
        /// Номер сторінки (з 1).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Кількість елементів на сторінці. Сервіс обмежує значення діапазоном від 1 до 48.
        /// </summary>
        public int PageSize { get; set; } = 12;
    }
}
