using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("menu_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalUrl).HasMaxLength(2048);
        b.Property(x => x.Anchor).HasMaxLength(256);
        b.HasOne(x => x.Menu)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentMenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TargetPage)
            .WithMany()
            .HasForeignKey(x => x.TargetPageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
