using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Europiyum.Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormEmailNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormRecipientEmails",
                table: "mail_settings",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotifyEmails",
                table: "form_definitions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendEmailOnSubmission",
                table: "form_definitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormRecipientEmails",
                table: "mail_settings");

            migrationBuilder.DropColumn(
                name: "NotifyEmails",
                table: "form_definitions");

            migrationBuilder.DropColumn(
                name: "SendEmailOnSubmission",
                table: "form_definitions");
        }
    }
}
