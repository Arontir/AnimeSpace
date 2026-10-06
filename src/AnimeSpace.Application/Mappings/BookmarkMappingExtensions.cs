using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    public static class BookmarkMappingExtensions
    {
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
