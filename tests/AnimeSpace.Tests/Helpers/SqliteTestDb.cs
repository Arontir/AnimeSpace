using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Tests.Helpers
{
    /// <summary>
    /// Тимчасова база SQLite у пам'яті для інтеграційних тестів.
    /// Кожен тест створює власну порожню базу зі схемою, побудованою з тієї самої моделі EF, що й у застосунку.
    /// База живе, доки відкрите з'єднання, тому воно закривається в <see cref="Dispose"/>.
    /// </summary>
    public sealed class SqliteTestDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        /// <summary>
        /// Основний контекст: його передають у репозиторій, який перевіряється.
        /// </summary>
        public ApplicationDbContext Context { get; }

        /// <summary>
        /// Відкриває з'єднання, вмикає зовнішні ключі та створює схему бази.
        /// </summary>
        public SqliteTestDb()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new ApplicationDbContext(_options);
            Context.Database.EnsureCreated();
        }

        /// <summary>
        /// Створює ще один контекст на ту саму базу. Імітує «інший запит»: у нього чистий трекер змін,
        /// тому через нього зручно перевіряти, що реально збережено в базі.
        /// </summary>
        public ApplicationDbContext CreateNewContext() => new(_options);

        /// <summary>
        /// Зберігає сутності в базі й очищає трекер змін, щоб тест починав з «чистого» контексту,
        /// як нові запити в застосунку.
        /// </summary>
        /// <param name="entities">Сутності для збереження (можна графом із навігаціями).</param>
        public async Task SeedAsync(params object[] entities)
        {
            Context.AddRange(entities);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Те саме, що <see cref="SeedAsync"/>, але для колекції однотипних сутностей.
        /// </summary>
        /// <param name="entities">Сутності для збереження.</param>
        public async Task SeedRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            Context.AddRange(entities.Cast<object>());
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Створює й зберігає користувача (потрібен для оцінок, закладок і коментарів через зовнішні ключі).
        /// </summary>
        /// <param name="name">Частина пошти до символу '@'.</param>
        public async Task<ApplicationUser> AddUserAsync(string name = "user")
        {
            var user = new ApplicationUser
            {
                UserName = $"{name}@mail.com",
                Email = $"{name}@mail.com"
            };

            await SeedAsync(user);
            return user;
        }

        /// <summary>
        /// Закриває контекст і з'єднання (база в пам'яті зникає).
        /// </summary>
        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
