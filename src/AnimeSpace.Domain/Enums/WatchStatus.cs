namespace AnimeSpace.Domain.Enums
{
    /// <summary>
    /// Статус тайтлу в особистому списку користувача.
    /// </summary>
    public enum WatchStatus
    {
        /// <summary>
        /// У планах.
        /// </summary>
        Planned = 1,

        /// <summary>
        /// Дивлюсь.
        /// </summary>
        Watching = 2,

        /// <summary>
        /// Переглянуто.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Покинуто.
        /// </summary>
        Dropped = 4
    }
}