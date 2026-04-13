using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class CompanyLanguageConfiguration : IEntityTypeConfiguration<CompanyLanguage>
{
    public void Configure(EntityTypeBuilder<CompanyLanguage> b)
    {
        b.ToTable("company_languages");
        b.HasKey(x => new { x.CompanyId, x.LanguageId });
        b.HasOne(x => x.Company)
            .WithMany(x => x.CompanyLanguages)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany(x => x.CompanyLanguages)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
