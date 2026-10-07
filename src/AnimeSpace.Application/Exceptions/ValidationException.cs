namespace AnimeSpace.Application.Exceptions
{
    /// <summary>
    /// Порушено бізнес-правило (некоректне посилання на плеєр, неприпустимий файл, дубль назви жанру тощо).
    /// У веб-шарі перетворюється на відповідь 400.
    /// </summary>
    /// <param name="message">Повідомлення для користувача.</param>
    public class ValidationException(string message) : Exception(message);
}
