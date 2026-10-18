using AnimeSpace.Domain.Entities;
using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Tests.Helpers
{
    /// <summary>
    /// Короткі фабрики сутностей для тестів, щоб не повторювати однакове створення об'єктів.
    /// </summary>
    internal static class TestData
    {
        /// <summary>
        /// Створює жанр із заданою назвою.
        /// </summary>
        /// <param name="name">Назва жанру.</param>
        public static Genre CreateGenre(string name) => new() { Name = name };

        /// <summary>
        /// Створює тайтл із жанрами. Усі необов'язкові поля мають розумні значення за замовчуванням.
        /// </summary>
        /// <param name="title">Назва.</param>
        /// <param name="year">Рік виходу.</param>
        /// <param name="status">Статус виходу.</param>
        /// <param name="genres">Жанри тайтлу.</param>
        public static Anime CreateAnime(string title, int year = 2020, AnimeStatus status = AnimeStatus.Finished, params Genre[] genres)
        {
            var anime = new Anime
            {
                Title = title,
                ReleaseYear = year,
                Status = status
            };

            foreach (var genre in genres)
            {
                anime.Genres.Add(genre);
            }

            return anime;
        }
    }
}
