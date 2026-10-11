namespace AnimeSpace.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// Налаштування початкового наповнення бази (секція "Seed" у конфігурації).
    /// </summary>
    public class SeedOptions
    {
        /// <summary>
        /// Назва секції в конфігурації.
        /// </summary>
        public const string SectionName = "Seed";

        /// <summary>
        /// Пошта (вона ж логін) адміністратора. Якщо не задана, адміністратор не створюється.
        /// </summary>
        public string? AdminEmail { get; set; }

        /// <summary>
        /// Пароль адміністратора. Має відповідати правилам паролів, налаштованим для Identity.
        /// </summary>
        public string? AdminPassword { get; set; }

        /// <summary>
        /// Чи додавати демонстраційні тайтли в порожню базу.
        /// </summary>
        public bool SeedDemoData { get; set; }
    }
}