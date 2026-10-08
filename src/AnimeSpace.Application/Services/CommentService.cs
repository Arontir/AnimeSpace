using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Application.Mappings;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Services
{
    /// <summary>
    /// Бізнес-логіка коментарів до тайтлів.
    /// </summary>
    /// <param name="comments">Репозиторій коментарів.</param>
    /// <param name="animes">Репозиторій тайтлів (для перевірки, що тайтл існує).</param>
    public class CommentService(ICommentRepository comments,IAnimeRepository animes) : ICommentService
    {
        /// <summary>
        /// Максимальна довжина коментаря.
        /// </summary>
        private const int MaxTextLength = 2000;

        /// <inheritdoc/>
        public async Task<IReadOnlyList<CommentDto>> GetByAnimeIdAsync(int animeId)
        {
            var list = await comments.GetByAnimeIdAsync(animeId);
            return list.Select(c => c.ToDto()).ToList();
        }

        /// <inheritdoc/>
        public async Task<CommentDto> AddAsync(string userId, int animeId, string text)
        {
            var trimmed = text?.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                throw new ValidationException("Коментар не може бути порожнім.");
            }

            if (trimmed.Length > MaxTextLength)
            {
                throw new ValidationException($"Коментар має бути не довшим за {MaxTextLength} символів.");
            }

            if (!await animes.ExistsAsync(animeId))
            {
                throw new NotFoundException("Аніме не знайдено.");
            }

            var comment = new Comment
            {
                Text = trimmed,
                UserId = userId,
                AnimeId = animeId,
                CreatedAt = DateTime.UtcNow
            };

            await comments.AddAsync(comment);

            var saved = await comments.GetByIdAsync(comment.Id) ?? throw new InvalidOperationException("Коментар не знайдено одразу після збереження.");

            return saved.ToDto();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int commentId, string userId, bool isAdmin)
        {
            var comment = await comments.GetByIdAsync(commentId) ?? throw new NotFoundException("Коментар не знайдено.");

            if (!isAdmin && comment.UserId != userId)
            {
                throw new ForbiddenException("Ви можете видаляти лише власні коментарі.");
            }

            await comments.DeleteAsync(comment);
        }
    }
}