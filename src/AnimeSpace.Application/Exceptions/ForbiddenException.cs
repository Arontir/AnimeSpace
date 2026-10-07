namespace AnimeSpace.Application.Exceptions
{
    /// <summary>
    /// Користувач не має прав на дію (наприклад, видалення чужого коментаря).
    /// У веб-шарі перетворюється на відповідь 403.
    /// </summary>
    /// <param name="message">Повідомлення для користувача.</param>
    public class ForbiddenException(string message) : Exception(message);
}
