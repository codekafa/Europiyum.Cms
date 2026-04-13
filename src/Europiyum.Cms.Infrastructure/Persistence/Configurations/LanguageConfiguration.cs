using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> b)
    {
        b.ToTable("languages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(16).IsRequired();
        b.Property(x => x.Name).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
    }
}
