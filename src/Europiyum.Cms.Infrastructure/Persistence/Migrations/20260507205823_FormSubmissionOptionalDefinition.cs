using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Europiyum.Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormSubmissionOptionalDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_form_submissions_form_definitions_FormDefinitionId",
                table: "form_submissions");

            migrationBuilder.AlterColumn<int>(
                name: "FormDefinitionId",
                table: "form_submissions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "form_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormKey",
                table: "form_submissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE form_submissions AS fs
                SET ""CompanyId"" = fd.""CompanyId"",
                    ""FormKey"" = fd.""Key""
                FROM form_definitions AS fd
                WHERE fs.""FormDefinitionId"" = fd.""Id""
                  AND fs.""CompanyId"" IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE form_submissions
                SET ""FormKey"" = ''
                WHERE ""FormKey"" IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "form_submissions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FormKey",
                table: "form_submissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_submissions_CompanyId_FormKey",
                table: "form_submissions",
                columns: new[] { "CompanyId", "FormKey" });

            migrationBuilder.AddForeignKey(
                name: "FK_form_submissions_companies_CompanyId",
                table: "form_submissions",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_form_submissions_form_definitions_FormDefinitionId",
                table: "form_submissions",
                column: "FormDefinitionId",
                principalTable: "form_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_form_submissions_companies_CompanyId",
                table: "form_submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_form_submissions_form_definitions_FormDefinitionId",
                table: "form_submissions");

            migrationBuilder.DropIndex(
                name: "IX_form_submissions_CompanyId_FormKey",
                table: "form_submissions");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "form_submissions");

            migrationBuilder.DropColumn(
                name: "FormKey",
                table: "form_submissions");

            migrationBuilder.AlterColumn<int>(
                name: "FormDefinitionId",
                table: "form_submissions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_form_submissions_form_definitions_FormDefinitionId",
                table: "form_submissions",
                column: "FormDefinitionId",
                principalTable: "form_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
