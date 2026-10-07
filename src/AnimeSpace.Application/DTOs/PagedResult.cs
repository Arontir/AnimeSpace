namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Одна сторінка результатів разом із даними для пагінації.
    /// </summary>
    /// <typeparam name="T">Тип елементів сторінки.</typeparam>
    public record PagedResult<T>
    {
        /// <summary>
        /// Елементи поточної сторінки.
        /// </summary>
        public required IReadOnlyList<T> Items { get; init; }
        
        /// <summary>
        /// Загальна кількість елементів за поточним фільтром (на всіх сторінках).
        /// </summary>
        public required int TotalCount { get; init; }

        /// <summary>
        /// Фактичний номер поточної сторінки.
        /// </summary>
        public required int PageNumber { get; init; }

        /// <summary>
        /// Фактичний розмір сторінки.
        /// </summary>
        public required int PageSize { get; init; }

        /// <summary>
        /// Загальна кількість сторінок. Обчислюється з <see cref="TotalCount"/> і <see cref="PageSize"/>.
        /// </summary>
        public int TotalPages => (PageSize > 0) 
            ? (int)Math.Ceiling(TotalCount / (double)PageSize) 
            : 0;
    }
}
