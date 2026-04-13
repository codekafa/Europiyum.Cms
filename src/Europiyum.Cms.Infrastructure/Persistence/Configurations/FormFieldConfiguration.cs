using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> b)
    {
        b.ToTable("form_fields");
        b.HasKey(x => x.Id);
        b.Property(x => x.FieldKey).HasMaxLength(128).IsRequired();
        b.Property(x => x.DefaultLabel).HasMaxLength(256);
        b.HasOne(x => x.FormDefinition)
            .WithMany(x => x.Fields)
            .HasForeignKey(x => x.FormDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
