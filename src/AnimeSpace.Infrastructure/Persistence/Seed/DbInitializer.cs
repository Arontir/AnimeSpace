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
            "Романтика", "Фантастика", "Спорт", "Детектив", "Повсякденність", "Ісекай"
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
                    Title = "Frieren: Beyond Journey's End",
                    Description = "Після перемоги над Королем Демонів ельфійка-чарівниця вирушає пізнавати людей.",
                    ReleaseYear = 2023,
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BZTI4ZGMxN2UtODlkYS00MTBjLWE1YzctYzc3NDViMGI0ZmJmXkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
                    PlayerUrl = "https://www.youtube.com/embed/videoseries?list=PLtdSPZNWT1AuEnWLElajRzm2DZhx1tVhd",
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-5),
                    Genres = Pick("Пригоди", "Фентезі", "Драма")
                },
                new()
                {
                    Title = "The Eminence in Shadow",
                    Description = "Хлопець, що мріє стати таємним очільником тіні, заснував вигадану організацію, яка виявилася справжньою.",
                    ReleaseYear = 2022,
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMDFmMWJlMWYtMDFkNi00OWViLTkzOGEtNDI0YWNmM2M5MzI5XkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
                    PlayerUrl = "https://www.youtube.com/embed/videoseries?list=PL3ongWuaWwZNBggs9x1Qcya3nRJzfMbkK",
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-4),
                    Genres = Pick("Екшн", "Комедія", "Фентезі", "Ісекай")
                },
                new()
                {
                    Title = "Chainsmoker Cat",
                    Description = "Дівчина-кішка намагається впоратися з важкою нікотиновою залежністю, постійними боргами та проблемою з роботою через свою пагубну звичку.",
                    ReleaseYear = 2026,
                    PosterUrl= "https://m.media-amazon.com/images/M/MV5BZTY5MzFmMzYtZjBhNS00ZGU5LTgzZWEtZWZmNDU2MDIwOThkXkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
                    PlayerUrl= "https://www.youtube.com/embed/videoseries?list=PL6GT3U72Ld9lYD_zI8HroRG81Y3jgO3jQ",
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-3),
                    Genres = Pick("Комедія", "Драма", "Повсякденність")
                },
                new()
                {
                    Title = "Delicious in Dungeon",
                    Description = "Шукачі пригод проходять підземелля, готуючи й поїдаючи монстрів, щоб урятувати подругу.",
                    ReleaseYear = 2024,
                    PosterUrl= "https://m.media-amazon.com/images/M/MV5BNmU3ZGFhMmYtNzczYy00NTE0LThkYjgtNWFiYTY2NzRkM2QzXkEyXkFqcGc@._V1_.jpg",
                    PlayerUrl= "https://ashdi.vip/serial/3110",
                    Status = AnimeStatus.Finished,
                    CreatedAt = now.AddMinutes(-2),
                    Genres = Pick("Пригоди", "Комедія", "Фентезі")
                },
                new()
                {
                    Title = "Reincarnated as a Sword",
                    Description = "Загиблий чоловік перероджується в розумний меч і стає наставником юної дівчинки-кішки.",
                    ReleaseYear = 2022,
                    PosterUrl= "https://m.media-amazon.com/images/M/MV5BNjdmMTA3ZWItNmRkZi00YWQ2LWEyMWMtMGFhZThhYTRmZmJlXkEyXkFqcGc@._V1_.jpg",
                    PlayerUrl= "https://ashdi.vip/serial/1960",
                    Status = AnimeStatus.Ongoing,
                    CreatedAt = now.AddMinutes(-1),
                    Genres = Pick("Екшн", "Пригоди", "Фентезі", "Ісекай")
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