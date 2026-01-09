using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalog_ProductAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TextPrompt = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductAttribute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_SpecificationAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    AllowFiltering = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEssential = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ShowOnProductPage = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_SpecificationAttribute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Common_City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsBillingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsShippingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsCityEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsZipCodeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_City", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Common_DeliveryTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorHexValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayLocate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDays = table.Column<int>(type: "integer", nullable: true),
                    MinDays = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_DeliveryTime", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Content_MediaFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    Alt = table.Column<string>(type: "text", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    MediaType = table.Column<string>(type: "text", nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content_MediaFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Content_Widget",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateUrl = table.Column<string>(type: "text", nullable: true),
                    EditUrl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    ViewComponentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content_Widget", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Content_WidgetZone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content_WidgetZone", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_ActivityLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivityLogTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_ActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_ActivityLogType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemKeyword = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_ActivityLogType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_EntityType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TargetActionName = table.Column<string>(type: "text", nullable: true),
                    TargetAreaName = table.Column<string>(type: "text", nullable: true),
                    TargetControllerName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_EntityType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductAttributeOptionsSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ProductAttributeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductAttributeOptionsSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductAttributeOptionsSet_Catalog_ProductAttribute~",
                        column: x => x.ProductAttributeId,
                        principalTable: "Catalog_ProductAttribute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_SpecificationAttributeOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Color = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NumberValue = table.Column<int>(type: "integer", nullable: false),
                    SpecificationAttributeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_SpecificationAttributeOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_SpecificationAttributeOption_Catalog_SpecificationA~",
                        column: x => x.SpecificationAttributeId,
                        principalTable: "Catalog_SpecificationAttribute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Common_District",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_District", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Common_District_Common_City_CityId",
                        column: x => x.CityId,
                        principalTable: "Common_City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MediaFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_Brand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_Brand_Content_MediaFile_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Content_MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShowOnHomePage = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeInMenu = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsRootParent = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    MediaFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_Category_Catalog_Category_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Catalog_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_Category_Content_MediaFile_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Content_MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Content_WidgetInstance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    HtmlData = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<byte>(type: "smallint", nullable: false),
                    CreateOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LatestUpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WidgetId = table.Column<string>(type: "text", nullable: true),
                    WidgetZoneId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content_WidgetInstance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Content_WidgetInstance_Content_WidgetZone_WidgetZoneId",
                        column: x => x.WidgetZoneId,
                        principalTable: "Content_WidgetZone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Content_WidgetInstance_Content_Widget_WidgetId",
                        column: x => x.WidgetId,
                        principalTable: "Content_Widget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Platform_UrlRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    EntityTypeId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_UrlRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Platform_UrlRecord_Platform_EntityType_EntityTypeId",
                        column: x => x.EntityTypeId,
                        principalTable: "Platform_EntityType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Identity_IdentityRoleClaim`1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_IdentityRoleClaim`1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Identity_IdentityRoleClaim`1_Platform_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Platform_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductAttributeOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PriceAdjustment = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductAttributeOptionsSetId = table.Column<int>(type: "integer", nullable: false),
                    WeightAdjustment = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductAttributeOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductAttributeOption_Catalog_ProductAttributeOpti~",
                        column: x => x.ProductAttributeOptionsSetId,
                        principalTable: "Catalog_ProductAttributeOptionsSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Common_Address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressLine1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AddressLine2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ZipCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: true),
                    DistrictId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Common_Address_Common_City_CityId",
                        column: x => x.CityId,
                        principalTable: "Common_City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Common_Address_Common_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Common_District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Sku = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Gtin = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    HasOptions = table.Column<bool>(type: "boolean", nullable: false),
                    IsAllowToOrder = table.Column<bool>(type: "boolean", nullable: false),
                    ShowOnHomePage = table.Column<bool>(type: "boolean", nullable: false),
                    HomePageDisplayOrder = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisibleIndividually = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SpecialPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SpecialPriceEndsUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SpecialPriceStartsUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedRatingSum = table.Column<int>(type: "integer", nullable: false),
                    NotApprovedRatingSum = table.Column<int>(type: "integer", nullable: false),
                    ApprovedReviewCount = table.Column<int>(type: "integer", nullable: false),
                    NotApprovedReviewCount = table.Column<int>(type: "integer", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<decimal>(type: "numeric", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Width = table.Column<decimal>(type: "numeric", nullable: false),
                    Length = table.Column<decimal>(type: "numeric", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_Product_Catalog_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Catalog_Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Platform_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    ExtensionData = table.Column<string>(type: "text", nullable: true),
                    ClientIdentity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastUserAgent = table.Column<string>(type: "text", nullable: true),
                    LastUserDeviceType = table.Column<string>(type: "text", nullable: true),
                    LastVisitedPage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LatestUpdateOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivityDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    BillingAddressId = table.Column<int>(type: "integer", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Platform_User_Common_Address_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalTable: "Common_Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Platform_User_Common_Address_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Common_Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductAttribute_Mapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttributeControlTypeId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ProductAttributeId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductAttribute_Mapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductAttribute_Mapping_Catalog_ProductAttribute_P~",
                        column: x => x.ProductAttributeId,
                        principalTable: "Catalog_ProductAttribute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductAttribute_Mapping_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<byte>(type: "smallint", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductCategory_Catalog_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Catalog_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductCategory_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    LinkedProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductLink_Catalog_Product_LinkedProductId",
                        column: x => x.LinkedProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductLink_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductSpecificationAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SpecificationAttributeOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductSpecificationAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductSpecificationAttribute_Catalog_Product_Produ~",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductSpecificationAttribute_Catalog_Specification~",
                        column: x => x.SpecificationAttributeOptionId,
                        principalTable: "Catalog_SpecificationAttributeOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductVariantAttributeCombination",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BasePriceAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    BasePriceBaseAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    DeliveryTimeId = table.Column<int>(type: "integer", nullable: true),
                    Gtin = table.Column<string>(type: "text", nullable: true),
                    Height = table.Column<decimal>(type: "numeric", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: true),
                    Width = table.Column<decimal>(type: "numeric", nullable: true),
                    Length = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    OldPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SpecialPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SpecialPriceEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SpecialPriceStarts = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HashCode = table.Column<int>(type: "integer", nullable: false),
                    RawAttributes = table.Column<string>(type: "text", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityUnitId = table.Column<int>(type: "integer", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductVariantAttributeCombination", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductVariantAttributeCombination_Catalog_Product_~",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductVariantAttributeCombination_Common_DeliveryT~",
                        column: x => x.DeliveryTimeId,
                        principalTable: "Common_DeliveryTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Content_ProductMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayOrder = table.Column<byte>(type: "smallint", nullable: false),
                    MediaFileId = table.Column<int>(type: "integer", nullable: true),
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content_ProductMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Content_ProductMedia_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Content_ProductMedia_Content_MediaFile_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Content_MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CommentText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ReviewerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductReview_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductReview_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Common_UserAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressType = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AddressId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_UserAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Common_UserAddress_Common_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Common_Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Common_UserAddress_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Identity_IdentityUserClaim`1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_IdentityUserClaim`1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Identity_IdentityUserClaim`1_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Identity_IdentityUserLogin`1",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_IdentityUserLogin`1", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_Identity_IdentityUserLogin`1_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Identity_IdentityUserToken`1",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_IdentityUserToken`1", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_Identity_IdentityUserToken`1_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Platform_UserRole",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Platform_UserRole_Platform_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Platform_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Platform_UserRole_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_ProductVariantAttributeValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPreSelected = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PriceAdjustment = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductVariantAttributeId = table.Column<int>(type: "integer", nullable: false),
                    WeightAdjustment = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductVariantAttributeValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductVariantAttributeValue_Catalog_ProductAttribu~",
                        column: x => x.ProductVariantAttributeId,
                        principalTable: "Catalog_ProductAttribute_Mapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_Reply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReplyText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReplyStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProductReviewId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_Reply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_Reply_Catalog_ProductReview_ProductReviewId",
                        column: x => x.ProductReviewId,
                        principalTable: "Catalog_ProductReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Catalog_Reply_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Brand_MediaFileId",
                table: "Catalog_Brand",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Category_MediaFileId",
                table: "Catalog_Category",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Category_ParentId",
                table: "Catalog_Category",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Product_BrandId",
                table: "Catalog_Product",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductAttribute_Mapping_ProductAttributeId",
                table: "Catalog_ProductAttribute_Mapping",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductAttribute_Mapping_ProductId",
                table: "Catalog_ProductAttribute_Mapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductAttributeOption_ProductAttributeOptionsSetId",
                table: "Catalog_ProductAttributeOption",
                column: "ProductAttributeOptionsSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductAttributeOptionsSet_ProductAttributeId",
                table: "Catalog_ProductAttributeOptionsSet",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductCategory_CategoryId",
                table: "Catalog_ProductCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductCategory_ProductId",
                table: "Catalog_ProductCategory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductLink_LinkedProductId",
                table: "Catalog_ProductLink",
                column: "LinkedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductLink_ProductId",
                table: "Catalog_ProductLink",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductReview_ProductId",
                table: "Catalog_ProductReview",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductReview_UserId",
                table: "Catalog_ProductReview",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductSpecificationAttribute_ProductId",
                table: "Catalog_ProductSpecificationAttribute",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductSpecificationAttribute_SpecificationAttribut~",
                table: "Catalog_ProductSpecificationAttribute",
                column: "SpecificationAttributeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductVariantAttributeCombination_DeliveryTimeId",
                table: "Catalog_ProductVariantAttributeCombination",
                column: "DeliveryTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductVariantAttributeCombination_ProductId",
                table: "Catalog_ProductVariantAttributeCombination",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductVariantAttributeValue_ProductVariantAttribut~",
                table: "Catalog_ProductVariantAttributeValue",
                column: "ProductVariantAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Reply_ProductReviewId",
                table: "Catalog_Reply",
                column: "ProductReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Reply_UserId",
                table: "Catalog_Reply",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_SpecificationAttributeOption_SpecificationAttribute~",
                table: "Catalog_SpecificationAttributeOption",
                column: "SpecificationAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Address_CityId",
                table: "Common_Address",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Address_DistrictId",
                table: "Common_Address",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_District_CityId",
                table: "Common_District",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_UserAddress_AddressId",
                table: "Common_UserAddress",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_UserAddress_UserId",
                table: "Common_UserAddress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_ProductMedia_MediaFileId",
                table: "Content_ProductMedia",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_ProductMedia_ProductId",
                table: "Content_ProductMedia",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_WidgetInstance_WidgetId",
                table: "Content_WidgetInstance",
                column: "WidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_WidgetInstance_WidgetZoneId",
                table: "Content_WidgetInstance",
                column: "WidgetZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Identity_IdentityRoleClaim`1_RoleId",
                table: "Identity_IdentityRoleClaim`1",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Identity_IdentityUserClaim`1_UserId",
                table: "Identity_IdentityUserClaim`1",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Identity_IdentityUserLogin`1_UserId",
                table: "Identity_IdentityUserLogin`1",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Platform_Role",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platform_UrlRecord_EntityTypeId",
                table: "Platform_UrlRecord",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Platform_User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Platform_User_BillingAddressId",
                table: "Platform_User",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Platform_User_ShippingAddressId",
                table: "Platform_User",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Platform_User",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platform_UserRole_RoleId",
                table: "Platform_UserRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Catalog_ProductAttributeOption");

            migrationBuilder.DropTable(
                name: "Catalog_ProductCategory");

            migrationBuilder.DropTable(
                name: "Catalog_ProductLink");

            migrationBuilder.DropTable(
                name: "Catalog_ProductSpecificationAttribute");

            migrationBuilder.DropTable(
                name: "Catalog_ProductVariantAttributeCombination");

            migrationBuilder.DropTable(
                name: "Catalog_ProductVariantAttributeValue");

            migrationBuilder.DropTable(
                name: "Catalog_Reply");

            migrationBuilder.DropTable(
                name: "Common_UserAddress");

            migrationBuilder.DropTable(
                name: "Content_ProductMedia");

            migrationBuilder.DropTable(
                name: "Content_WidgetInstance");

            migrationBuilder.DropTable(
                name: "Identity_IdentityRoleClaim`1");

            migrationBuilder.DropTable(
                name: "Identity_IdentityUserClaim`1");

            migrationBuilder.DropTable(
                name: "Identity_IdentityUserLogin`1");

            migrationBuilder.DropTable(
                name: "Identity_IdentityUserToken`1");

            migrationBuilder.DropTable(
                name: "Platform_ActivityLog");

            migrationBuilder.DropTable(
                name: "Platform_ActivityLogType");

            migrationBuilder.DropTable(
                name: "Platform_Setting");

            migrationBuilder.DropTable(
                name: "Platform_UrlRecord");

            migrationBuilder.DropTable(
                name: "Platform_UserRole");

            migrationBuilder.DropTable(
                name: "Catalog_ProductAttributeOptionsSet");

            migrationBuilder.DropTable(
                name: "Catalog_Category");

            migrationBuilder.DropTable(
                name: "Catalog_SpecificationAttributeOption");

            migrationBuilder.DropTable(
                name: "Common_DeliveryTime");

            migrationBuilder.DropTable(
                name: "Catalog_ProductAttribute_Mapping");

            migrationBuilder.DropTable(
                name: "Catalog_ProductReview");

            migrationBuilder.DropTable(
                name: "Content_WidgetZone");

            migrationBuilder.DropTable(
                name: "Content_Widget");

            migrationBuilder.DropTable(
                name: "Platform_EntityType");

            migrationBuilder.DropTable(
                name: "Platform_Role");

            migrationBuilder.DropTable(
                name: "Catalog_SpecificationAttribute");

            migrationBuilder.DropTable(
                name: "Catalog_ProductAttribute");

            migrationBuilder.DropTable(
                name: "Catalog_Product");

            migrationBuilder.DropTable(
                name: "Platform_User");

            migrationBuilder.DropTable(
                name: "Catalog_Brand");

            migrationBuilder.DropTable(
                name: "Common_Address");

            migrationBuilder.DropTable(
                name: "Content_MediaFile");

            migrationBuilder.DropTable(
                name: "Common_District");

            migrationBuilder.DropTable(
                name: "Common_City");
        }
    }
}
