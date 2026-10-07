using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    /// <summary>
    /// Перетворення закладки на DTO.
    /// </summary>
    public static class BookmarkMappingExtensions
    {
        /// <summary>
        /// Створює елемент списку закладок. Тайтл (Anime) має бути завантажений через Include.
        /// </summary>
        /// <param name="bookmark">Закладка.</param>
        public static UserBookmarkDto ToDto(this Bookmark bookmark) => new UserBookmarkDto()
        {
            AnimeId = bookmark.AnimeId,
            AnimeTitle = bookmark.Anime.Title,
            AnimePosterUrl = bookmark.Anime.PosterUrl,
            Status = bookmark.Status,
            UpdatedAt = bookmark.UpdatedAt
        };
    }
}
