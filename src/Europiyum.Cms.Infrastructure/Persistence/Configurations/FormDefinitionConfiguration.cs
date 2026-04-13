using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> b)
    {
        b.ToTable("form_definitions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.Key).HasMaxLength(128).IsRequired();
        b.Property(x => x.NotifyEmails).HasMaxLength(1024);
        b.HasIndex(x => new { x.CompanyId, x.Key }).IsUnique();
        b.HasOne(x => x.Company)
            .WithMany(x => x.FormDefinitions)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
