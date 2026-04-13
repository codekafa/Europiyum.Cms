using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Europiyum.Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PageFormDefinitionAndAppearance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormDefinitionId",
                table: "pages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pages_FormDefinitionId",
                table: "pages",
                column: "FormDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_pages_form_definitions_FormDefinitionId",
                table: "pages",
                column: "FormDefinitionId",
                principalTable: "form_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pages_form_definitions_FormDefinitionId",
                table: "pages");

            migrationBuilder.DropIndex(
                name: "IX_pages_FormDefinitionId",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "FormDefinitionId",
                table: "pages");
        }
    }
}
