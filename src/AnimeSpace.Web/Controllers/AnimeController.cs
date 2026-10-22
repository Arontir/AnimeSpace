using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Web.Extensions;
using AnimeSpace.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Controllers
{
    /// <summary>
    /// Каталог тайтлів і сторінка перегляду тайтлу.
    /// </summary>
    /// <param name="animeService">Сервіс тайтлів.</param>
    /// <param name="genreService">Сервіс жанрів (для списку жанрів у фільтрі).</param>
    /// <param name="commentService">Сервіс коментарів.</param>
    /// <param name="bookmarkService">Сервіс закладок (статус тайтлу в списку поточного користувача).</param>
    /// <param name="ratingService">Сервіс оцінок (оцінка поточного користувача).</param>
    public class AnimeController(
        IAnimeService animeService,
        IGenreService genreService,
        ICommentService commentService,
        IBookmarkService bookmarkService,
        IRatingService ratingService) : Controller
    {
        /// <summary>
        /// Каталог: пошук за назвою, фільтри за жанром, роком і статусом, сортування, пагінація.
        /// </summary>
        /// <param name="filter">Параметри з рядка запиту; сервіс сам виправляє некоректні значення.</param>
        public async Task<IActionResult> Catalog([FromQuery] AnimeFilterDto filter)
        {
            var pagedResult = await animeService.GetCatalogAsync(filter);
            var genres = await genreService.GetAllAsync();

            var model = new CatalogViewModel
            {
                AnimePagedList = pagedResult,
                AvailableGenres = genres,
                CurrentFilter = filter
            };

            return View(model);
        }

        /// <summary>
        /// Сторінка тайтлу: опис, жанри, рейтинг, плеєр і коментарі. Для авторизованого користувача додатково
        /// підставляються його статус у списку та оцінка (для гостя ці поля порожні).
        /// Якщо тайтлу немає, сервіс кидає NotFoundException, а GlobalExceptionFilter показує сторінку 404.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        public async Task<IActionResult> Details(int id)
        {
            var anime = await animeService.GetDetailsAsync(id);
            var comments = await commentService.GetByAnimeIdAsync(id);

            WatchStatus? bookmarkStatus = null;
            int? userRating = null;
            string? userId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.GetUserId();
                bookmarkStatus = await bookmarkService.GetStatusAsync(userId, id);
                userRating = await ratingService.GetUserRatingAsync(userId, id);
            }

            var model = new AnimeDetailsViewModel
            {
                Anime = anime,
                Comments = comments,
                CurrentUserBookmarkStatus = bookmarkStatus,
                CurrentUserRating = userRating,
                CurrentUserId = userId
            };

            return View(model);
        }
    }
}
