using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class MenuItemTranslationConfiguration : IEntityTypeConfiguration<MenuItemTranslation>
{
    public void Configure(EntityTypeBuilder<MenuItemTranslation> b)
    {
        b.ToTable("menu_item_translations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Label).HasMaxLength(256).IsRequired();
        b.HasIndex(x => new { x.MenuItemId, x.LanguageId }).IsUnique();
        b.HasOne(x => x.MenuItem)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany(x => x.MenuItemTranslations)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
