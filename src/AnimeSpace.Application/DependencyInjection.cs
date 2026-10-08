using AnimeSpace.Application.Abstractions.Services;
using AnimeSpace.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeSpace.Application
{
    /// <summary>
    /// Підключення сервісів шару Application до DI.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Реєструє всі сервіси бізнес-логіки зі строком життя Scoped.
        /// </summary>
        /// <param name="services">Колекція сервісів застосунку.</param>
        /// <returns>Та сама колекція, щоб виклики можна було з'єднувати в ланцюжок.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAnimeService, AnimeService>();
            services.AddScoped<IBookmarkService, BookmarkService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IStatsService, StatsService>();

            return services;
        }
    }
}