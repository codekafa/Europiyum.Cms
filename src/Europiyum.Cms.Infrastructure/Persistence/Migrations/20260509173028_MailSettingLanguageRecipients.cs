using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Europiyum.Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MailSettingLanguageRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mail_setting_language_recipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MailSettingId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    RecipientEmails = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mail_setting_language_recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mail_setting_language_recipients_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mail_setting_language_recipients_mail_settings_MailSettingId",
                        column: x => x.MailSettingId,
                        principalTable: "mail_settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mail_setting_language_recipients_LanguageId",
                table: "mail_setting_language_recipients",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_mail_setting_language_recipients_MailSettingId_LanguageId",
                table: "mail_setting_language_recipients",
                columns: new[] { "MailSettingId", "LanguageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mail_setting_language_recipients");
        }
    }
}
