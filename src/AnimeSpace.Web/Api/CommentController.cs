using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Domain.Constants;
using AnimeSpace.Web.Extensions;
using AnimeSpace.Web.ViewModels.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Api
{
    /// <summary>
    /// JSON-ендпоїнти коментарів (викликаються з anime-details.js через fetch).
    /// Список коментарів тут не віддається: він приходить разом зі сторінкою тайтлу.
    /// </summary>
    /// <param name="commentService">Сервіс коментарів.</param>
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CommentController(ICommentService commentService) : ControllerBase
    {
        /// <summary>
        /// Додає коментар від імені поточного користувача.
        /// </summary>
        /// <param name="request">Тайтл і текст коментаря.</param>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddCommentRequest request)
        {
            var userId = User.GetUserId();

            var resultDto = await commentService.AddAsync(
                userId: userId,
                animeId: request.AnimeId,
                text: request.Text);

            return Ok(resultDto);
        }

        /// <summary>
        /// Видаляє коментар: адміністратор може видалити будь-який, звичайний користувач лише власний
        /// (інакше сервіс кидає ForbiddenException, а фільтр відповідає 403).
        /// </summary>
        /// <param name="id">Ідентифікатор коментаря.</param>
        [HttpPost]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsInRole(AppRoles.Admin);

            await commentService.DeleteAsync(
                commentId: id,
                userId: userId,
                isAdmin: isAdmin);

            return Ok();
        }
    }
}
