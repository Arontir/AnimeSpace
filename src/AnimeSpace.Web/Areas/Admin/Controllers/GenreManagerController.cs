using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.DTOs;
using AnimeSpace.Application.Exceptions;
using AnimeSpace.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimeSpace.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Керування жанрами: список із формою додавання, перейменування, видалення. Доступний лише адміністраторам.
    /// Якщо жанру немає, сервіс кидає NotFoundException, а фільтр показує сторінку 404.
    /// </summary>
    /// <param name="genreService">Сервіс жанрів.</param>
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class GenreManagerController(IGenreService genreService) : Controller
    {
        /// <summary>
        /// Список жанрів і форма додавання.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var genres = await genreService.GetAllAsync();

            return View(genres);
        }

        /// <summary>
        /// Додає жанр. Результат (успіх або помилка, наприклад дубль назви) передається через TempData,
        /// після чого завжди відбувається перенаправлення на список.
        /// </summary>
        /// <param name="dto">Дані форми.</param>
        [HttpPost]
        public async Task<IActionResult> Create(GenreCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Будь ласка, перевірте правильність заповнення форми.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                await genreService.CreateAsync(dto);
                TempData["Success"] = "Жанр успішно створено.";
            }
            catch (ValidationException e)
            {
                TempData["Error"] = e.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Форма перейменування.
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var genre = await genreService.GetByIdAsync(id);

            return View(genre);
        }

        /// <summary>
        /// Перейменовує жанр. Помилки (порожня назва, дубль) показуються над формою.
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        /// <param name="dto">Нова назва.</param>
        [HttpPost]
        public async Task<IActionResult> Edit(int id, GenreCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(new GenreDto { Id = id, Name = dto.Name });
            }

            try
            {
                await genreService.UpdateAsync(id, dto);
                TempData["Success"] = "Жанр успішно оновлено.";

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError(string.Empty, e.Message);

                return View(new GenreDto { Id = id, Name = dto.Name });
            }
        }

        /// <summary>
        /// Видаляє жанр (тайтли залишаються, але втрачають цей жанр).
        /// </summary>
        /// <param name="id">Ідентифікатор жанру.</param>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await genreService.DeleteAsync(id);
            TempData["Success"] = "Жанр успішно видалено.";

            return RedirectToAction(nameof(Index));
        }
    }
}
