using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class PageComponentConfiguration : IEntityTypeConfiguration<PageComponent>
{
    public void Configure(EntityTypeBuilder<PageComponent> b)
    {
        b.ToTable("page_components");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.PageId, x.ComponentItemId }).IsUnique();
        b.HasOne(x => x.Page)
            .WithMany(x => x.PageComponents)
            .HasForeignKey(x => x.PageId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ComponentItem)
            .WithMany(x => x.PageComponents)
            .HasForeignKey(x => x.ComponentItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
