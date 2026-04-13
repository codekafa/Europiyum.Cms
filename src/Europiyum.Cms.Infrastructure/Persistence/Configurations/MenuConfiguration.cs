using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> b)
    {
        b.ToTable("menus");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(128);
        b.HasIndex(x => new { x.CompanyId, x.Kind, x.Name });
        b.HasOne(x => x.Company)
            .WithMany(x => x.Menus)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
