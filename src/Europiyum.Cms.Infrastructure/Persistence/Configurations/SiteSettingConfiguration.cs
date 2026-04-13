using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> b)
    {
        b.ToTable("site_settings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(128).IsRequired();
        b.HasIndex(x => new { x.CompanyId, x.Key }).IsUnique();
        b.HasOne(x => x.Company)
            .WithMany(x => x.SiteSettings)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
