using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Web.ViewModels
{
    /// <summary>
    /// Дані для головної сторінки (Home/Index).
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Останні додані тайтли (для слайдера).
        /// </summary>
        public IReadOnlyList<AnimeCardDto> LatestReleases { get; set; } = [];

        /// <summary>
        /// Тайтли, що виходять зараз (блок «Онгоїнги»).
        /// </summary>
        public IReadOnlyList<AnimeCardDto> Ongoing { get; set; } = [];

        /// <summary>
        /// Тайтли з найвищим рейтингом (блок «Топ-10»).
        /// </summary>
        public IReadOnlyList<AnimeCardDto> TopRated { get; set; } = [];
    }
}
