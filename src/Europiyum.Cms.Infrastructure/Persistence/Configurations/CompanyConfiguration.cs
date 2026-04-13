using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable("companies");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.Code).HasMaxLength(64).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        b.Property(x => x.PrimaryDomain).HasMaxLength(512);
        b.Property(x => x.HomepageVariantKey).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasOne(x => x.DefaultLanguage)
            .WithMany()
            .HasForeignKey(x => x.DefaultLanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
