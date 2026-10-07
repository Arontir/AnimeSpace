namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Дані форми редагування тайтлу: поля створення плюс ідентифікатор і поточний постер.
    /// </summary>

    public class AnimeUpdateDto : AnimeCreateDto
    {
        /// <summary>
        /// Ідентифікатор тайтлу, що редагується).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Поточний постер лише для показу превʼю у формі (може бути локальним шляхом "/uploads/...").
        /// </summary>
        public string? CurrentPosterUrl { get; set; }
    }
}
