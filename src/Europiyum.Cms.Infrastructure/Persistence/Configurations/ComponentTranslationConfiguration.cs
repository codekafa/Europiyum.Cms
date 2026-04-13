using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class ComponentTranslationConfiguration : IEntityTypeConfiguration<ComponentTranslation>
{
    public void Configure(EntityTypeBuilder<ComponentTranslation> b)
    {
        b.ToTable("component_translations");
        b.HasKey(x => x.Id);
        b.Property(x => x.ButtonText).HasMaxLength(256);
        b.Property(x => x.ButtonUrl).HasMaxLength(1024);
        b.HasIndex(x => new { x.ComponentItemId, x.LanguageId }).IsUnique();
        b.HasOne(x => x.ComponentItem)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.ComponentItemId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany(x => x.ComponentTranslations)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
