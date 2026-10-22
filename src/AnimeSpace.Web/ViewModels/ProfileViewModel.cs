using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Web.ViewModels
{
    /// <summary>
    /// Дані для особистого кабінету (Profile/Index).
    /// </summary>
    public class ProfileViewModel
    {
        /// <summary>
        /// Ім'я користувача (логін).
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Дата реєстрації (UTC).
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Усі закладки користувача; у розмітці групуються за статусом по вкладках.
        /// </summary>
        public IReadOnlyList<UserBookmarkDto> Bookmarks { get; set; } = [];
    }
}
