using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Web.ViewModels
{
    /// <summary>
    /// Дані для сторінки перегляду тайтлу (Anime/Details).
    /// Поля про поточного користувача (статус закладки, оцінка) заповнюються лише для авторизованих.
    /// </summary>
    public class AnimeDetailsViewModel
    {
        /// <summary>
        /// Повна інформація про тайтл.
        /// </summary>
        public AnimeDetailsDto Anime { get; set; } = null!;

        /// <summary>
        /// Коментарі до тайтлу (до 100 останніх, від новіших).
        /// </summary>
        public IReadOnlyList<CommentDto> Comments { get; set; } = [];

        /// <summary>
        /// Статус тайтлу в списку поточного користувача; <c>null</c>, якщо тайтлу немає в його списках.
        /// </summary>
        public WatchStatus? CurrentUserBookmarkStatus { get; set; }

        /// <summary>
        /// Оцінка, яку поточний користувач поставив тайтлу; <c>null</c>, якщо не оцінював.
        /// </summary>
        public int? CurrentUserRating { get; set; }

        /// <summary>
        /// Ідентифікатор поточного користувача (потрібен, щоб показати кнопку видалення власних коментарів); <c>null</c> для гостя.
        /// </summary>
        public string? CurrentUserId { get; set; }
    }
}
