using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class MailSettingConfiguration : IEntityTypeConfiguration<MailSetting>
{
    public void Configure(EntityTypeBuilder<MailSetting> b)
    {
        b.ToTable("mail_settings");
        b.HasKey(x => x.Id);
        b.Property(x => x.SmtpHost).HasMaxLength(256).IsRequired();
        b.Property(x => x.UserName).HasMaxLength(256);
        b.Property(x => x.Password).HasMaxLength(512);
        b.Property(x => x.SenderEmail).HasMaxLength(256).IsRequired();
        b.Property(x => x.SenderName).HasMaxLength(256).IsRequired();
        b.Property(x => x.FormRecipientEmails).HasMaxLength(1024);
        b.HasIndex(x => x.CompanyId).IsUnique();
        b.HasOne(x => x.Company)
            .WithOne(x => x.MailSetting)
            .HasForeignKey<MailSetting>(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
