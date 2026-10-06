using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Mappings
{
    public static class CommentMappingExtensions
    {
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
