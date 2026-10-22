using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Web.Extensions;
using AnimeSpace.Web.ViewModels.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Api
{
    /// <summary>
    /// JSON-ендпоїнт оцінювання тайтлів (викликається з anime-details.js через fetch).
    /// </summary>
    /// <param name="ratingService">Сервіс оцінок.</param>
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class RatingController(IRatingService ratingService) : ControllerBase
    {
        /// <summary>
        /// Зберігає оцінку поточного користувача (нову або змінює наявну) і повертає нові підсумки тайтлу.
        /// </summary>
        /// <param name="request">Тайтл і оцінка від 1 до 10.</param>
        [HttpPost]
        public async Task<IActionResult> Set([FromBody] RateRequest request)
        {
            var userId = User.GetUserId();

            var resultDto = await ratingService.SetRatingAsync(
                userId: userId,
                animeId: request.AnimeId,
                score: request.Score);

            return Ok(resultDto);
        }
    }
}
