using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class SeoMetadataConfiguration : IEntityTypeConfiguration<SeoMetadata>
{
    public void Configure(EntityTypeBuilder<SeoMetadata> b)
    {
        b.ToTable("seo_metadata");
        b.HasKey(x => x.Id);
        b.Property(x => x.MetaTitle).HasMaxLength(512);
        b.Property(x => x.MetaDescription).HasMaxLength(1024);
        b.Property(x => x.MetaKeywords).HasMaxLength(512);
        b.Property(x => x.CanonicalUrl).HasMaxLength(1024);
        b.Property(x => x.OgTitle).HasMaxLength(512);
        b.Property(x => x.OgDescription).HasMaxLength(1024);
        b.Property(x => x.Robots).HasMaxLength(128);
        b.HasIndex(x => new { x.PageId, x.LanguageId }).IsUnique();
        b.HasOne(x => x.Page)
            .WithMany(x => x.SeoEntries)
            .HasForeignKey(x => x.PageId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OgImageMedia)
            .WithMany()
            .HasForeignKey(x => x.OgImageMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
