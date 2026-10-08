using AnimeSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimeSpace.Infrastructure.Persistence.Configurations
{
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.HasIndex(r => new { r.UserId, r.AnimeId }).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint(
                name: "CK_Ratings_Score",
                sql: $"[Score] >= {Rating.MinScore} AND [Score] <= {Rating.MaxScore}"
            ));

            builder.HasOne(r => r.Anime)
                .WithMany()
                .HasForeignKey(r => r.AnimeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
