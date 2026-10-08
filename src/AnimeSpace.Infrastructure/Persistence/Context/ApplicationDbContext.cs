using AnimeSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnimeSpace.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Anime> Animes => Set<Anime>();
        public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Rating> Ratings => Set<Rating>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(assembly: typeof(ApplicationDbContext).Assembly);
        }
    }
}
