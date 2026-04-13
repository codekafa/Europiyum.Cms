using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class ComponentItemConfiguration : IEntityTypeConfiguration<ComponentItem>
{
    public void Configure(EntityTypeBuilder<ComponentItem> b)
    {
        b.ToTable("component_items");
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Company)
            .WithMany(x => x.Components)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ComponentType)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ComponentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PrimaryMedia)
            .WithMany()
            .HasForeignKey(x => x.PrimaryMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
