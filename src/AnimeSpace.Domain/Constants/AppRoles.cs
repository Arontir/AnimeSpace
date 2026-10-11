namespace AnimeSpace.Domain.Constants
{
    /// <summary>
    /// Назви ролей застосунку. Окремої ролі для глядача немає:
    /// глядач — це будь-який авторизований користувач без ролі <see cref="Admin"/>.
    /// </summary>
    public class AppRoles
    {
        /// <summary>
        /// Роль адміністратора: доступ до панелі керування (область Admin).
        /// </summary>
        public const string Admin = "Admin";
    }
}
