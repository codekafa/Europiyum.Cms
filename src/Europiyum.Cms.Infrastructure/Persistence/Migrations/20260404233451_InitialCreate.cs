using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Europiyum.Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "component_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsRtl = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryDomain = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DefaultLanguageId = table.Column<int>(type: "integer", nullable: false),
                    HomepageVariantKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_companies_languages_DefaultLanguageId",
                        column: x => x.DefaultLanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "company_languages",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_languages", x => new { x.CompanyId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_company_languages_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_company_languages_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_definitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_definitions_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mail_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    SmtpHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Password = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    SenderEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mail_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mail_settings_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    AltText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_files_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menus_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    PageType = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pages_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_site_settings_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_fields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OptionsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_fields_form_definitions_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "form_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    SubmitterIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_submissions_form_definitions_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "form_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "component_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ComponentTypeId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JsonPayload = table.Column<string>(type: "text", nullable: true),
                    PrimaryMediaId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_component_items_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_component_items_component_types_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalTable: "component_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_component_items_media_files_PrimaryMediaId",
                        column: x => x.PrimaryMediaId,
                        principalTable: "media_files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuId = table.Column<int>(type: "integer", nullable: false),
                    ParentMenuItemId = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    LinkType = table.Column<int>(type: "integer", nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Anchor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetPageId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_items_menu_items_ParentMenuItemId",
                        column: x => x.ParentMenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_menu_items_menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_items_pages_TargetPageId",
                        column: x => x.TargetPageId,
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "page_translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_page_translations_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_page_translations_pages_PageId",
                        column: x => x.PageId,
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seo_metadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    MetaTitle = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CanonicalUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OgTitle = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OgDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OgImageMediaId = table.Column<int>(type: "integer", nullable: true),
                    Robots = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seo_metadata_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seo_metadata_media_files_OgImageMediaId",
                        column: x => x.OgImageMediaId,
                        principalTable: "media_files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_seo_metadata_pages_PageId",
                        column: x => x.PageId,
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "component_translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComponentItemId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BodyHtml = table.Column<string>(type: "text", nullable: true),
                    ButtonText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ButtonUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExtraJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_component_translations_component_items_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "component_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_component_translations_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "home_page_sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedComponentItemId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_page_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_home_page_sections_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_home_page_sections_component_items_LinkedComponentItemId",
                        column: x => x.LinkedComponentItemId,
                        principalTable: "component_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "page_components",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    ComponentItemId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_page_components_component_items_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "component_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_page_components_pages_PageId",
                        column: x => x.PageId,
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_item_translations_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_menu_item_translations_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "home_page_section_translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HomePageSectionId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    BodyHtml = table.Column<string>(type: "text", nullable: true),
                    JsonPayload = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_page_section_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_home_page_section_translations_home_page_sections_HomePageS~",
                        column: x => x.HomePageSectionId,
                        principalTable: "home_page_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_home_page_section_translations_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_companies_Code",
                table: "companies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_DefaultLanguageId",
                table: "companies",
                column: "DefaultLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_companies_Slug",
                table: "companies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_languages_LanguageId",
                table: "company_languages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_component_items_CompanyId",
                table: "component_items",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_component_items_ComponentTypeId",
                table: "component_items",
                column: "ComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_component_items_PrimaryMediaId",
                table: "component_items",
                column: "PrimaryMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_component_translations_ComponentItemId_LanguageId",
                table: "component_translations",
                columns: new[] { "ComponentItemId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_component_translations_LanguageId",
                table: "component_translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_component_types_Key",
                table: "component_types",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_definitions_CompanyId_Key",
                table: "form_definitions",
                columns: new[] { "CompanyId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_fields_FormDefinitionId",
                table: "form_fields",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_form_submissions_FormDefinitionId",
                table: "form_submissions",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_home_page_section_translations_HomePageSectionId_LanguageId",
                table: "home_page_section_translations",
                columns: new[] { "HomePageSectionId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_home_page_section_translations_LanguageId",
                table: "home_page_section_translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_home_page_sections_CompanyId_SectionKey_SortOrder",
                table: "home_page_sections",
                columns: new[] { "CompanyId", "SectionKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_home_page_sections_LinkedComponentItemId",
                table: "home_page_sections",
                column: "LinkedComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_languages_Code",
                table: "languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mail_settings_CompanyId",
                table: "mail_settings",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_files_CompanyId",
                table: "media_files",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_translations_LanguageId",
                table: "menu_item_translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_translations_MenuItemId_LanguageId",
                table: "menu_item_translations",
                columns: new[] { "MenuItemId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_MenuId",
                table: "menu_items",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_ParentMenuItemId",
                table: "menu_items",
                column: "ParentMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_TargetPageId",
                table: "menu_items",
                column: "TargetPageId");

            migrationBuilder.CreateIndex(
                name: "IX_menus_CompanyId_Kind_Name",
                table: "menus",
                columns: new[] { "CompanyId", "Kind", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_page_components_ComponentItemId",
                table: "page_components",
                column: "ComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_page_components_PageId_ComponentItemId",
                table: "page_components",
                columns: new[] { "PageId", "ComponentItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_translations_LanguageId",
                table: "page_translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_page_translations_PageId_LanguageId",
                table: "page_translations",
                columns: new[] { "PageId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pages_CompanyId_Slug",
                table: "pages",
                columns: new[] { "CompanyId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seo_metadata_LanguageId",
                table: "seo_metadata",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_seo_metadata_OgImageMediaId",
                table: "seo_metadata",
                column: "OgImageMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_seo_metadata_PageId_LanguageId",
                table: "seo_metadata",
                columns: new[] { "PageId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_site_settings_CompanyId_Key",
                table: "site_settings",
                columns: new[] { "CompanyId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_languages");

            migrationBuilder.DropTable(
                name: "component_translations");

            migrationBuilder.DropTable(
                name: "form_fields");

            migrationBuilder.DropTable(
                name: "form_submissions");

            migrationBuilder.DropTable(
                name: "home_page_section_translations");

            migrationBuilder.DropTable(
                name: "mail_settings");

            migrationBuilder.DropTable(
                name: "menu_item_translations");

            migrationBuilder.DropTable(
                name: "page_components");

            migrationBuilder.DropTable(
                name: "page_translations");

            migrationBuilder.DropTable(
                name: "seo_metadata");

            migrationBuilder.DropTable(
                name: "site_settings");

            migrationBuilder.DropTable(
                name: "form_definitions");

            migrationBuilder.DropTable(
                name: "home_page_sections");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "component_items");

            migrationBuilder.DropTable(
                name: "menus");

            migrationBuilder.DropTable(
                name: "pages");

            migrationBuilder.DropTable(
                name: "component_types");

            migrationBuilder.DropTable(
                name: "media_files");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "languages");
        }
    }
}
