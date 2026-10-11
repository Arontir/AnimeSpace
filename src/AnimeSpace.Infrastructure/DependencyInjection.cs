using AnimeSpace.Application.Abstractions.Repositories;
using AnimeSpace.Application.Abstractions.Storage;
using AnimeSpace.Infrastructure.Persistence.Context;
using AnimeSpace.Infrastructure.Persistence.Seed;
using AnimeSpace.Infrastructure.Repositories;
using AnimeSpace.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeSpace.Infrastructure
{
    /// <summary>
    /// Підключення шару Infrastructure до контейнера залежностей (DI).
    /// Викликається один раз у Program.cs: <c>builder.Services.AddInfrastructure(...)</c>.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Реєструє контекст бази даних, репозиторії, сховище файлів та налаштування.
        /// Усі сервіси мають строк життя Scoped: репозиторії працюють із DbContext, який теж живе один запит.
        /// </summary>
        /// <param name="services">Колекція сервісів застосунку.</param>
        /// <param name="configuration">Конфігурація.</param>
        /// <param name="webRootPath">Абсолютний шлях до wwwroot (builder.Environment.WebRootPath): сюди зберігаються постери.</param>
        /// <returns>Та сама колекція, щоб виклики можна було з'єднувати в ланцюжок.</returns>
        /// <exception cref="InvalidOperationException">У конфігурації немає рядка підключення 'DefaultConnection'.</exception>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            string webRootPath)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Не знайдено рядок підключення 'DefaultConnection'.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IAnimeRepository, AnimeRepository>();
            services.AddScoped<IBookmarkRepository, BookmarkRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<IStatsRepository, StatsRepository>();

            services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
            services.PostConfigure<FileStorageOptions>(o => o.WebRootPath = webRootPath);
            services.AddScoped<IFileService, LocalFileService>();

            services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

            return services;
        }
    }
}
