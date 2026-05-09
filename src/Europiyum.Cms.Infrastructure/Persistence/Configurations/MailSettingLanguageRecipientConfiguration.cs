using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class MailSettingLanguageRecipientConfiguration : IEntityTypeConfiguration<MailSettingLanguageRecipient>
{
    public void Configure(EntityTypeBuilder<MailSettingLanguageRecipient> b)
    {
        b.ToTable("mail_setting_language_recipients");
        b.HasKey(x => x.Id);
        b.Property(x => x.RecipientEmails).HasMaxLength(1024);
        b.HasIndex(x => new { x.MailSettingId, x.LanguageId }).IsUnique();
        b.HasOne(x => x.MailSetting)
            .WithMany(x => x.LanguageRecipients)
            .HasForeignKey(x => x.MailSettingId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language)
            .WithMany(x => x.MailSettingLanguageRecipients)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
