using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class ComponentTypeConfiguration : IEntityTypeConfiguration<ComponentType>
{
    public void Configure(EntityTypeBuilder<ComponentType> b)
    {
        b.ToTable("component_types");
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(64).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.Key).IsUnique();
    }
}
