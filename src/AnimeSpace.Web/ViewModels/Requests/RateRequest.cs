using AnimeSpace.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AnimeSpace.Web.ViewModels.Requests
{
    /// <summary>
    /// Тіло JSON-запиту POST /Rating/Set.
    /// </summary>
    public class RateRequest
    {
        /// <summary>
        /// Ідентифікатор тайтлу, який оцінюється.
        /// </summary>
        public required int AnimeId { get; set; }

        /// <summary>
        /// Оцінка від 1 до 10.
        /// </summary>
        [Range(Rating.MinScore, Rating.MaxScore, ErrorMessage = "Оцінка має бути в межах від 1 до 10.")]
        public required int Score { get; set; }
    }
}
