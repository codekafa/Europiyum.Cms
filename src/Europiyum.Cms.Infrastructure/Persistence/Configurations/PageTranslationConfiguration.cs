using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class PageTranslationConfiguration : IEntityTypeConfiguration<PageTranslation>
{
    public void Configure(EntityTypeBuilder<PageTranslation> b)
    {
        b.ToTable("page_translations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(512).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(256);
        b.Property(x => x.BreadcrumbHeading).HasMaxLength(512);
        b.Property(x => x.BreadcrumbBackgroundPath).HasMaxLength(512);
        b.HasIndex(x => new { x.PageId, x.LanguageId }).IsUnique();
        b.HasOne(x => x.Page)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.PageId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany(x => x.PageTranslations)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
