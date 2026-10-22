using System.ComponentModel.DataAnnotations;

namespace AnimeSpace.Web.ViewModels.Requests
{
    /// <summary>
    /// Тіло JSON-запиту POST /Comment/Add.
    /// </summary>
    public class AddCommentRequest
    {
        /// <summary>
        /// Ідентифікатор тайтлу, до якого додається коментар.
        /// </summary>
        public required int AnimeId { get; set; }

        /// <summary>
        /// Текст коментаря (до 2000 символів).
        /// </summary>
        [Required(ErrorMessage = "Текст коментаря є обов'язковим.")]
        [StringLength(2000, ErrorMessage = "Довжина коментаря не повинна перевищувати 2000 символів.")]
        public required string Text { get; set; }
    }
}
