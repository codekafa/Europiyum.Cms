using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class HomePageSectionTranslationConfiguration : IEntityTypeConfiguration<HomePageSectionTranslation>
{
    public void Configure(EntityTypeBuilder<HomePageSectionTranslation> b)
    {
        b.ToTable("home_page_section_translations");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.HomePageSectionId, x.LanguageId }).IsUnique();
        b.HasOne(x => x.HomePageSection)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.HomePageSectionId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany(x => x.HomePageSectionTranslations)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
