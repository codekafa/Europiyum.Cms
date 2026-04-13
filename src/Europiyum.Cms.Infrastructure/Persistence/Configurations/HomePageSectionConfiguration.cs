using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class HomePageSectionConfiguration : IEntityTypeConfiguration<HomePageSection>
{
    public void Configure(EntityTypeBuilder<HomePageSection> b)
    {
        b.ToTable("home_page_sections");
        b.HasKey(x => x.Id);
        b.Property(x => x.SectionKey).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.CompanyId, x.SectionKey, x.SortOrder });
        b.HasOne(x => x.Company)
            .WithMany(x => x.HomePageSections)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.LinkedComponentItem)
            .WithMany(x => x.HomePageSections)
            .HasForeignKey(x => x.LinkedComponentItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
