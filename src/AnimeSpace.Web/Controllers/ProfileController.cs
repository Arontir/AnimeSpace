using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Web.Extensions;
using AnimeSpace.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Controllers
{
    /// <summary>
    /// Особистий кабінет: дані профілю та списки закладок. Доступний лише авторизованим користувачам.
    /// </summary>
    /// <param name="userManager">Менеджер користувачів Identity (для імені та дати реєстрації).</param>
    /// <param name="bookmarkService">Сервіс закладок.</param>
    [Authorize]
    public class ProfileController(UserManager<ApplicationUser> userManager, IBookmarkService bookmarkService) : Controller
    {
        /// <summary>
        /// Сторінка кабінету.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound();
            }

            var bookmarks = await bookmarkService.GetUserBookmarksAsync(userId);

            var model = new ProfileViewModel
            {
                UserName = user.UserName ?? "Користувач",
                RegisteredAt = user.RegisteredAt,
                Bookmarks = bookmarks
            };

            return View(model);
        }
    }
}
