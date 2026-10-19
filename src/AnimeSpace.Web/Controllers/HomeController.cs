using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Web.Models;
using AnimeSpace.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AnimeSpace.Web.Controllers
{
    /// <summary>
    /// Головна сторінка сайту та сторінка помилки.
    /// </summary>
    /// <param name="animeService">Сервіс тайтлів.</param>
    public class HomeController(IAnimeService animeService) : Controller
    {
        /// <summary>
        /// Головна сторінка: слайдер з новинками, блок «Онгоїнги» та топ за рейтингом.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var latest = await animeService.GetLatestAsync(5);

            var ongoingFilter = new AnimeFilterDto
            {
                Status = AnimeStatus.Ongoing,
                SortBy = AnimeSortBy.Newest,
                PageSize = 8
            };

            var ongoingResult = await animeService.GetCatalogAsync(ongoingFilter);

            var topRated = await animeService.GetTopRatedAsync(10);

            var model = new HomeViewModel
            {
                LatestReleases = latest,
                Ongoing = ongoingResult.Items,
                TopRated = topRated
            };

            return View(model);
        }

        /// <summary>
        /// Сторінка помилки (підключається через UseExceptionHandler поза режимом розробки).
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
