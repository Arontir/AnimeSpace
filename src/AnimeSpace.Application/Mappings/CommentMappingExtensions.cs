using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    /// <summary>
    /// Перетворення коментаря на DTO.
    /// </summary>
    public static class CommentMappingExtensions
    {
        /// <summary>
        /// Створює коментар для показу. Ім'я автора — частина логіна до символу '@'. Автор (User) має бути завантажений через Include.
        /// </summary>
        /// <param name="comment">Коментар.</param>
        public static CommentDto ToDto(this Comment comment)
        {
            var rawUserName = comment.User?.UserName ?? string.Empty;
            var atIndex = rawUserName.IndexOf('@');
            var displayName = atIndex >= 0 ? rawUserName[..atIndex] : rawUserName;

            return new CommentDto()
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                UserName = displayName
            };
        }
    }
}
