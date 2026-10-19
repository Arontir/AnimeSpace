using AnimeSpace.Application.DTOs;
using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Web.Extensions
{
    /// <summary>
    /// Українські підписи для перелічень (enum), щоб розмітка не містила однакових switch у кількох місцях.
    /// Використання в Razor: <c>@anime.Status.ToDisplay()</c>.
    /// </summary>
    public static class DisplayExtensions
    {
        /// <summary>
        /// Підпис статусу виходу.
        /// </summary>
        public static string ToDisplay(this AnimeStatus status) => status switch
        {
            AnimeStatus.Ongoing => "Онгоїнг",
            AnimeStatus.Finished => "Завершено",
            _ => status.ToString()
        };

        /// <summary>
        /// Підпис статусу в особистому списку.
        /// </summary>
        public static string ToDisplay(this WatchStatus status) => status switch
        {
            WatchStatus.Planned => "У планах",
            WatchStatus.Watching => "Дивлюсь",
            WatchStatus.Completed => "Переглянуто",
            WatchStatus.Dropped => "Покинуто",
            _ => status.ToString()
        };

        /// <summary>
        /// Підпис способу сортування каталогу.
        /// </summary>
        public static string ToDisplay(this AnimeSortBy sortBy) => sortBy switch
        {
            AnimeSortBy.Newest => "Спочатку нові",
            AnimeSortBy.TopRated => "За рейтингом",
            AnimeSortBy.TitleAsc => "За назвою",
            AnimeSortBy.ReleaseYearDesc => "За роком виходу",
            _ => sortBy.ToString()
        };
    }
}
