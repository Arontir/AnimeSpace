using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Web.Areas.Admin.ViewModels
{
    /// <summary>
    /// Дані форми створення та редагування тайтлу в адмінці (спільна розмітка _AnimeForm).
    /// </summary>
    public class AnimeFormViewModel
    {
        /// <summary>
        /// Поля тайтлу. Під час створення Id = 0; AnimeUpdateDto успадковує AnimeCreateDto,
        /// тому ту саму модель приймають і CreateAsync, і UpdateAsync.
        /// </summary>
        public AnimeUpdateDto Anime { get; set; } = new();

        /// <summary>
        /// Завантажений файл постера (необов'язковий).
        /// </summary>
        public IFormFile? PosterFile { get; set; }

        /// <summary>
        /// Усі жанри для прапорців у формі. Не надходить з форми, тому контролер заповнює її щоразу, коли показує форму.
        /// </summary>
        public IReadOnlyList<GenreDto> AllGenres { get; set; } = [];
    }
}
