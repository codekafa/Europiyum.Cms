using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> b)
    {
        b.ToTable("media_files");
        b.HasKey(x => x.Id);
        b.Property(x => x.OriginalFileName).HasMaxLength(512).IsRequired();
        b.Property(x => x.StoredFileName).HasMaxLength(512).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        b.Property(x => x.RelativePath).HasMaxLength(1024).IsRequired();
        b.Property(x => x.AltText).HasMaxLength(512);
        b.HasOne(x => x.Company)
            .WithMany(x => x.MediaFiles)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
