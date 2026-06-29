using LINTelligent.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LINTelligent.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.CodeSnippet)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Language)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired();
    }
}
