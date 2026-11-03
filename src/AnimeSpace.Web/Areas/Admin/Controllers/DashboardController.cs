using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Головна сторінка адмінки зі статистикою сайту. Доступна лише адміністраторам.
    /// </summary>
    /// <param name="statsService">Сервіс статистики.</param>
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class DashboardController(IStatsService statsService) : Controller
    {
        /// <summary>
        /// Статистика: кількість користувачів, тайтлів, жанрів і коментарів.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var dto = await statsService.GetDashboardStatsAsync();
            return View(dto);
        }
    }
}
