using System.Security.Claims;

namespace AnimeSpace.Web.Extensions
{
    /// <summary>
    /// Допоміжні методи для поточного користувача (User у контролерах і представленнях).
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Повертає ідентифікатор поточного користувача (claim NameIdentifier).
        /// Викликається лише в діях, захищених [Authorize]: там ідентифікатор гарантовано є.
        /// </summary>
        /// <param name="user">Поточний користувач.</param>
        /// <exception cref="InvalidOperationException">Ідентифікатор відсутній (виклик для неавторизованого користувача).</exception>
        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("Відсутній ідентифікатор користувача (User ID).");
    }
}
