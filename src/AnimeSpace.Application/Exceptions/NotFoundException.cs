namespace AnimeSpace.Application.Exceptions
{
    /// <summary>
    /// Запитаний запис не знайдено. У веб-шарі перетворюється на відповідь 404.
    /// </summary>
    /// <param name="message">Повідомлення для користувача.</param>
    public class NotFoundException(string message) : Exception(message);
}
