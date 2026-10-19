using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Web.ViewModels
{
    /// <summary>
    /// Дані для сторінки каталогу (Anime/Catalog): результати пошуку, список жанрів для фільтра та поточні параметри фільтра.
    /// </summary>
    public class CatalogViewModel
    {
        /// <summary>
        /// Поточна сторінка карток тайтлів разом з даними для пагінації.
        /// </summary>
        public PagedResult<AnimeCardDto> AnimePagedList { get; set; } = null!;

        /// <summary>
        /// Усі жанри для випадного списку у формі фільтрів.
        /// </summary>
        public IReadOnlyList<GenreDto> AvailableGenres { get; set; } = [];

        /// <summary>
        /// Параметри фільтра, з якими відкрито сторінку (щоб форма показувала вибрані значення).
        /// </summary>
        public AnimeFilterDto CurrentFilter { get; set; } = null!;
    }
}
