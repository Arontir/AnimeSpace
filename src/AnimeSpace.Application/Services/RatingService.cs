using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Services
{
    /// <summary>
    /// Бізнес-логіка оцінювання тайтлів.
    /// Сервіс вирішує, створювати оцінку чи оновлювати наявну;
    /// </summary>
    /// <param name="ratings">Репозиторій оцінок.</param>
    /// <param name="animes">Репозиторій тайтлів (для перевірки, що тайтл існує).</param>
    public class RatingService(IRatingRepository ratings, IAnimeRepository animes) : IRatingService
    {
        /// <inheritdoc/>
        public async Task<RatingResultDto> SetRatingAsync(string userId, int animeId, int score)
        {
            if (score < Rating.MinScore || score > Rating.MaxScore)
            {
                throw new ValidationException($"Оцінка має бути від {Rating.MinScore} до {Rating.MaxScore}.");
            }

            if (!await animes.ExistsAsync(animeId))
            {
                throw new NotFoundException("Аніме не знайдено.");
            }

            var rating = await ratings.GetAsync(userId, animeId);

            if (rating is null)
            {
                await ratings.AddAsync(new Rating
                {
                    UserId = userId,
                    AnimeId = animeId,
                    Score = score
                });
            }
            else
            {
                rating.Score = score;
                await ratings.UpdateAsync(rating);
            }

            return await ratings.RecalculateAverageRatingAsync(animeId);
        }

        /// <inheritdoc/>
        public async Task<int?> GetUserRatingAsync(string userId, int animeId)
        {
            var rating = await ratings.GetAsync(userId, animeId);
            return rating?.Score;
        }
    }
}