using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Entities;

namespace AnimeSpace.Application.Abstractions.Repositories
{
    public interface IRatingRepository
    {
        public Task<Rating?> GetAsync(string userId, int animeId);

        public Task AddAsync(Rating rating);

        public Task UpdateAsync(Rating rating);

        public Task<RatingResultDto> RecalculateAverageRatingAsync(int animeId);
    }
}
