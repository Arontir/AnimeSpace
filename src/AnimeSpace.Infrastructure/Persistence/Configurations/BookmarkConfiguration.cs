using AnimeSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimeSpace.Infrastructure.Persistence.Configurations
{
    public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
    {
        public void Configure(EntityTypeBuilder<Bookmark> builder)
        {
            builder.HasIndex(b => new { b.UserId, b.AnimeId }).IsUnique();

            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Anime)
                .WithMany()
                .HasForeignKey(b => b.AnimeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
