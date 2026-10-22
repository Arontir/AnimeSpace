using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Web.Extensions;
using AnimeSpace.Web.ViewModels.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Api
{
    /// <summary>
    /// JSON-ендпоїнти особистих списків (викликаються з bookmarks.js через fetch).
    /// [ApiController] автоматично повертає 400 при невалідному тілі запиту; бізнес-помилки
    /// (NotFoundException, ValidationException) перетворює на JSON GlobalExceptionFilter.
    /// </summary>
    /// <param name="bookmarkService">Сервіс закладок.</param>
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BookmarkController(IBookmarkService bookmarkService) : ControllerBase
    {
        /// <summary>
        /// Встановлює статус тайтлу в списку поточного користувача (створює закладку або змінює наявну).
        /// </summary>
        /// <param name="request">Тайтл і новий статус.</param>
        [HttpPost]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
        {
            var userId = User.GetUserId();

            await bookmarkService.SetStatusAsync(
                userId: userId,
                animeId: request.AnimeId,
                status: request.Status);

            return Ok();
        }

        /// <summary>
        /// Видаляє тайтл зі списків поточного користувача.
        /// </summary>
        /// <param name="animeId">Ідентифікатор тайтлу.</param>
        [HttpPost]
        public async Task<IActionResult> Delete([FromQuery] int animeId)
        {
            var userId = User.GetUserId();

            await bookmarkService.DeleteAsync(
                userId: userId,
                animeId: animeId);

            return Ok();
        }
    }
}
