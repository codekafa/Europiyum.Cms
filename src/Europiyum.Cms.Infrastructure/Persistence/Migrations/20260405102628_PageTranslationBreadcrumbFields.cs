using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Europiyum.Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PageTranslationBreadcrumbFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BreadcrumbBackgroundPath",
                table: "page_translations",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BreadcrumbHeading",
                table: "page_translations",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreadcrumbBackgroundPath",
                table: "page_translations");

            migrationBuilder.DropColumn(
                name: "BreadcrumbHeading",
                table: "page_translations");
        }
    }
}
