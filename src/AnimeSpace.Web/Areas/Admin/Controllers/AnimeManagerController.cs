using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Domain.Constants;
using AnimeSpace.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Керування тайтлами: список, створення, редагування (з файлом постера та посиланням на плеєр), видалення.
    /// Доступний лише адміністраторам. Якщо тайтлу немає, сервіс кидає NotFoundException, а фільтр показує сторінку 404.
    /// </summary>
    /// <param name="animeService">Сервіс тайтлів.</param>
    /// <param name="genreService">Сервіс жанрів (для прапорців жанрів у формі).</param>
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class AnimeManagerController(IAnimeService animeService, IGenreService genreService) : Controller
    {
        /// <summary>
        /// Таблиця тайтлів із пагінацією (15 на сторінку).
        /// </summary>
        /// <param name="page">Номер сторінки.</param>
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new AnimeFilterDto { Page = page, PageSize = 15 };
            var catalog = await animeService.GetCatalogAsync(filter);

            return View(catalog);
        }

        /// <summary>
        /// Порожня форма створення.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new AnimeFormViewModel
            {
                AllGenres = await genreService.GetAllAsync()
            };

            return View(model);
        }

        /// <summary>
        /// Створює тайтл із даних форми. Бізнес-помилки сервісу (некоректне посилання, неприпустимий файл тощо)
        /// показуються над формою. Ліміт запиту трохи більший за ліміт файлу (5 МБ), бо в запиті є й інші поля.
        /// </summary>
        /// <param name="model">Дані форми.</param>
        [HttpPost]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Create(AnimeFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                await using var stream = model.PosterFile?.OpenReadStream();

                try
                {
                    await animeService.CreateAsync(model.Anime, ToUpload(model.PosterFile, stream));
                    TempData["Success"] = "Аніме успішно додано.";

                    return RedirectToAction(nameof(Index));
                }
                catch (ValidationException e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            model.AllGenres = await genreService.GetAllAsync();

            return View(model);
        }

        /// <summary>
        /// Форма редагування з поточними даними тайтлу.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = new AnimeFormViewModel
            {
                Anime = await animeService.GetForEditAsync(id),
                AllGenres = await genreService.GetAllAsync()
            };

            return View(model);
        }

        /// <summary>
        /// Зберігає зміни тайтлу. Ідентифікатор з форми порівнюється з адресою, щоб не можна було підмінити чужий запис.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу з адреси.</param>
        /// <param name="model">Дані форми.</param>
        [HttpPost]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Edit(int id, AnimeFormViewModel model)
        {
            if (model.Anime.Id != id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await using var stream = model.PosterFile?.OpenReadStream();

                try
                {
                    await animeService.UpdateAsync(model.Anime, ToUpload(model.PosterFile, stream));
                    TempData["Success"] = "Аніме успішно оновлено.";

                    return RedirectToAction(nameof(Index));
                }
                catch (ValidationException e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            model.AllGenres = await genreService.GetAllAsync();

            return View(model);
        }

        /// <summary>
        /// Видаляє тайтл разом з оцінками, коментарями, закладками та файлом постера.
        /// </summary>
        /// <param name="id">Ідентифікатор тайтлу.</param>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await animeService.DeleteAsync(id);

            TempData["Success"] = "Аніме видалено.";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Перетворює файл форми на DTO для сервісу. Без файлу повертає <c>null</c>.
        /// </summary>
        /// <param name="file">Файл із форми.</param>
        /// <param name="stream">Відкритий потік цього файлу.</param>
        private static FileUploadDto? ToUpload(IFormFile? file, Stream? stream)
            => (file is null || stream is null)
                ? null
                : new FileUploadDto { Content = stream, FileName = file.FileName };
    }
}
