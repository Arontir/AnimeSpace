namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Порядок сортування каталогу.
    /// </summary>
    public enum AnimeSortBy
    {
        /// <summary>
        /// Спочатку нові (за датою додавання, від новіших).
        /// </summary>
        Newest = 1,

        /// <summary>
        /// За рейтингом, від найвищого.
        /// </summary>
        TopRated = 2,

        /// <summary>
        /// За назвою, за алфавітом.
        /// </summary>
        TitleAsc = 3,

        /// <summary>
        /// За роком виходу, від найновішого.
        /// </summary>
        ReleaseYearDesc = 4
    }
}
