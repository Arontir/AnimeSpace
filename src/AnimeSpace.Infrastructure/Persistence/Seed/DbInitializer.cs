using AnimeSpace.Domain.Constants;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnimeSpace.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// Початкове наповнення бази під час запуску застосунку: міграції, роль і обліковий запис адміністратора,
    /// базові жанри та демонстраційні тайтли. Усі кроки безпечні для повторного запуску:
    /// те, що вже є в базі, повторно не створюється.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Жанри, які створюються в порожній базі.
        /// </summary>
        private static readonly string[] DefaultGenres =
        [
            "Екшн", "Пригоди", "Комедія", "Драма", "Фентезі",
            "Романтика", "Фантастика", "Спорт", "Детектив", "Повсякденність"
        ];

        /// <summary>
        /// Виконує всі кроки наповнення. Викликається один раз під час запуску, ДО app.Run().
        /// Власний scope створюється всередині, тому передавати можна кореневий провайдер (app.Services).
        /// </summary>
        /// <param name="services">Провайдер сервісів застосунку.</param>
        /// <exception cref="InvalidOperationException">Не вдалося створити роль або адміністратора (наприклад, пароль не підходить під правила Identity).</exception>
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var db = provider.GetRequiredService<ApplicationDbContext>();
            var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var users = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var seed = provider.GetRequiredService<IOptions<SeedOptions>>().Value;

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            await db.Database.MigrateAsync();

            await EnsureAdminAsync(roles, users, seed, logger);

            await SeedGenresAsync(db);

            if (seed.SeedDemoData)
            {
                await SeedDemoAnimeAsync(db);
            }
        }

        
        /// <summary>
        /// Створює роль Admin (якщо її немає) та адміністратора з налаштувань Seed.
        /// Якщо пошту або пароль не вказано, обліковий запис не створюється, а в журнал пишеться попередження.
        /// </summary>
        private static async Task EnsureAdminAsync(
            RoleManager<IdentityRole> roles,
            UserManager<ApplicationUser> users,
            SeedOptions seed,
            ILogger logger)
        {
            if (!await roles.RoleExistsAsync(AppRoles.Admin))
            {
                ThrowIfFailed(await roles.CreateAsync(new IdentityRole(AppRoles.Admin)), "створити роль адміністратора");
            }

            if (string.IsNullOrWhiteSpace(seed.AdminEmail) || string.IsNullOrWhiteSpace(seed.AdminPassword))
            {
                logger.LogWarning("Seed:AdminEmail або Seed:AdminPassword не задано: обліковий запис адміністратора не створено.");
                return;
            }

            var admin = await users.FindByEmailAsync(seed.AdminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = seed.AdminEmail,
                    Email = seed.AdminEmail,
                    EmailConfirmed = true
                };

                ThrowIfFailed(await users.CreateAsync(admin, seed.AdminPassword), "створити адміністратора");
            }

            if (!await users.IsInRoleAsync(admin, AppRoles.Admin))
            {
                ThrowIfFailed(await users.AddToRoleAsync(admin, AppRoles.Admin), "призначити адміністратору роль");
            }
        }

        /// <summary>
        /// Додає базові жанри, якщо таблиця жанрів порожня.
        /// </summary>
        private static async Task SeedGenresAsync(ApplicationDbContext db)
        {
            if (await db.Genres.AnyAsync())
            {
                return;
            }

            db.Genres.AddRange(DefaultGenres.Select(name => new Genre { Name = name }));
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Додає кілька демонстраційних тайтлів, якщо в базі немає жодного.
        /// ToDo: Посилання на плеєр поки порожні: їх варто підібрати й перевірити в iframe окремо.
        /// </summary>
        private static async Task SeedDemoAnimeAsync(ApplicationDbContext db)
        {
            if (await db.Animes.AnyAsync())
            {
                return;
            }

            var genres = await db.Genres.ToDictionaryAsync(g => g.Name);

            List<Genre> Pick(params string[] names)
                => names.Where(genres.ContainsKey).Select(name => genres[name]).ToList();

            var now = DateTime.UtcNow;

            var demo = new List<Anime>
            {
                new()
                {
                    Title = "Cowboy Bebop",
                    Description = "Мисливці за головами подорожують Сонячною системою, переслідуючи злочинців і власне минуле.",
                    ReleaseYear = 1998,
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-5),
                    Genres = Pick("Екшн", "Фантастика", "Пригоди")
                },
                new()
                {
                    Title = "Attack on Titan",
                    Description = "Людство ховається за велетенськими стінами від титанів, які пожирають людей.",
                    ReleaseYear = 2013,
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-4),
                    Genres = Pick("Екшн", "Драма", "Фентезі")
                },
                new()
                {
                    Title = "Haikyu!!",
                    Description = "Невисокий, але завзятий хлопець приєднується до шкільної волейбольної команди.",
                    ReleaseYear = 2014,
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-3),
                    Genres = Pick("Спорт", "Комедія", "Драма")
                },
                new()
                {
                    Title = "Spy x Family",
                    Description = "Шпигун, кілерка й дівчинка-телепат вдають із себе звичайну родину.",
                    ReleaseYear = 2022,
                    Status = AnimeStatus.Ongoing,
                    CreatedAt = now.AddMinutes(-2),
                    Genres = Pick("Екшн", "Комедія")
                },
                new()
                {
                    Title = "Frieren: Beyond Journey's End",
                    Description = "Після перемоги над Королем Демонів ельфійка-чарівниця вирушає пізнавати людей.",
                    ReleaseYear = 2023,
                    Status = AnimeStatus.Ongoing,
                    CreatedAt = now.AddMinutes(-1),
                    Genres = Pick("Пригоди", "Фентезі", "Драма")
                }
            };

            db.Animes.AddRange(demo);
            await db.SaveChangesAsync();
        }

        /// <summary>Кидає зрозумілу помилку, якщо операція Identity не вдалася.</summary>
        /// <param name="result">Результат операції Identity.</param>
        /// <param name="action">Що намагалися зробити (для тексту помилки).</param>
        private static void ThrowIfFailed(IdentityResult result, string action)
        {
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Не вдалося {action}: {errors}");
            }
        }
    }
}