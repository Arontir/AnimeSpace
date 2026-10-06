using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    public interface IRatingService
    {
        public Task<RatingResultDto> SetRatingAsync(string userId, int animeId, int score);

        public Task<int?> GetUserRatingAsync(string userId, int animeId);
    }
}
