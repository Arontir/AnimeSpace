using Microsoft.AspNetCore.Identity;

namespace AnimeSpace.Domain.Entities
{
    /// <summary>
    /// Користувач сайту. Розширює стандартного користувача ASP.NET Core Identity.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Дата реєстрації (UTC).
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
