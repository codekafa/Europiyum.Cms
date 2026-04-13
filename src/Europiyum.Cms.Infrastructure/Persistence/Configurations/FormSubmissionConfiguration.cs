using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Europiyum.Cms.Infrastructure.Persistence.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> b)
    {
        b.ToTable("form_submissions");
        b.HasKey(x => x.Id);
        b.Property(x => x.PayloadJson).IsRequired();
        b.Property(x => x.SubmitterIp).HasMaxLength(64);
        b.HasOne(x => x.FormDefinition)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.FormDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
