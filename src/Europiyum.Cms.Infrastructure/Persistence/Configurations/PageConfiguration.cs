using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> b)
    {
        b.ToTable("pages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Slug).HasMaxLength(256).IsRequired();
        b.Property(x => x.TemplateKey).HasMaxLength(128);
        b.HasIndex(x => new { x.CompanyId, x.Slug }).IsUnique();
        b.HasOne(x => x.Company)
            .WithMany(x => x.Pages)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.FormDefinition)
            .WithMany()
            .HasForeignKey(x => x.FormDefinitionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
