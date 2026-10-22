using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Web.ViewModels.Requests
{
    /// <summary>
    /// Тіло JSON-запиту POST /Bookmark/ChangeStatus.
    /// Допустимість статусу перевіряє BookmarkService (з JSON може прийти будь-яке число).
    /// </summary>
    public class ChangeStatusRequest
    {
        /// <summary>
        /// Ідентифікатор тайтлу.
        /// </summary>
        public required int AnimeId { get; set; }

        /// <summary>
        /// Новий статус тайтлу в списку користувача.
        /// </summary>
        public required WatchStatus Status { get; set; }
    }
}
