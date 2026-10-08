using AnimeSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimeSpace.Infrastructure.Persistence.Configurations
{
    public class AnimeConfiguration : IEntityTypeConfiguration<Anime>
    {
        public void Configure(EntityTypeBuilder<Anime> builder)
        {
            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(a => a.Title);

            builder.Property(a => a.Description)
                .HasMaxLength(4000);

            builder.Property(a => a.PosterUrl)
                .HasMaxLength(500);

            builder.Property(a => a.PlayerUrl)
                .HasMaxLength(1000);

            builder.HasIndex(a => a.AverageRating);

            builder.HasMany(a => a.Genres)
                .WithMany(g => g.Animes)
                .UsingEntity(j => j.ToTable("AnimeGenres"));
        }
    }
}
