using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalog_DiscountBadge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_DiscountBadge", x => x.Id);
                });

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
                    Name = table.Column<string>(type: "text", nullable: true)
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
                name: "Common_Label",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_Label", x => x.Id);
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
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content_MediaFile", x => x.Id);
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
                name: "Platform_ExternalIdentityLogin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoginProvider = table.Column<string>(type: "text", nullable: true),
                    ProviderKey = table.Column<string>(type: "text", nullable: true),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_ExternalIdentityLogin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "Platform_ThemeVariable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Theme = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_ThemeVariable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platform_UrlRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform_UrlRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_Discount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DiscountType = table.Column<int>(type: "integer", nullable: false),
                    UsePercentage = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    StartsOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCouponRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CouponCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CouponUsageType = table.Column<int>(type: "integer", nullable: false),
                    AppliedTimes = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    BadgeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_Discount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_Discount_Catalog_DiscountBadge_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Catalog_DiscountBadge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    CityId = table.Column<int>(type: "integer", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Catalog_Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
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
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    HasDiscountsApplied = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "Platform_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemAccount = table.Column<bool>(type: "boolean", nullable: false),
                    SystemName = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExtensionData = table.Column<string>(type: "text", nullable: true),
                    ClientIdentity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastUserAgent = table.Column<string>(type: "text", nullable: true),
                    LastUserDeviceType = table.Column<string>(type: "text", nullable: true),
                    LastVisitedPage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    DiscountCouponCode = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LatestUpdateOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivityDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    BillingAddressId = table.Column<int>(type: "integer", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "integer", nullable: true)
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
                    MaxAddToCartNumber = table.Column<int>(type: "integer", nullable: false),
                    MinAddToCartNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HasDiscountsApplied = table.Column<bool>(type: "boolean", nullable: false),
                    BasePriceAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    BasePriceBaseAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    CombinationDisplayBehaviour = table.Column<int>(type: "integer", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    AttributeCombinationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayStockQuantity = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveryTimeId = table.Column<int>(type: "integer", nullable: true),
                    Sku = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    QuantityUnitId = table.Column<int>(type: "integer", nullable: false),
                    Gtin = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    HasOptions = table.Column<bool>(type: "boolean", nullable: false),
                    ShowOnHomePage = table.Column<bool>(type: "boolean", nullable: false),
                    HomePageDisplayOrder = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisibleIndividually = table.Column<bool>(type: "boolean", nullable: false),
                    IsShippingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Discount_CategoryDiscount_Mapping",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    DiscountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discount_CategoryDiscount_Mapping", x => new { x.CategoryId, x.DiscountId });
                    table.ForeignKey(
                        name: "FK_Discount_CategoryDiscount_Mapping_Catalog_Category_Category~",
                        column: x => x.CategoryId,
                        principalTable: "Catalog_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discount_CategoryDiscount_Mapping_Catalog_Discount_Discount~",
                        column: x => x.DiscountId,
                        principalTable: "Catalog_Discount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Checkout_Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ShippingAddressId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethodSystemName = table.Column<string>(type: "text", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    SubtotalRounded = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderDiscount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkout_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checkout_Order_Common_Address_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Common_Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checkout_Order_Platform_User_UserId",
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
                name: "UserAddresses",
                columns: table => new
                {
                    User_Id = table.Column<int>(type: "integer", nullable: false),
                    Address_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAddresses", x => new { x.User_Id, x.Address_Id });
                    table.ForeignKey(
                        name: "FK_UserAddresses_Common_Address_Address_Id",
                        column: x => x.Address_Id,
                        principalTable: "Common_Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAddresses_Platform_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "Catalog_ProductBrand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_ProductBrand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductBrand_Catalog_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Catalog_Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_ProductBrand_Catalog_Product_ProductId",
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
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
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
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    HashCode = table.Column<int>(type: "integer", nullable: false),
                    RawAttributes = table.Column<string>(type: "jsonb", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityUnitId = table.Column<int>(type: "integer", nullable: true),
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
                name: "Common_ProductLabel",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    LabelId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Common_ProductLabel", x => new { x.LabelId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_Common_ProductLabel_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Common_ProductLabel_Common_Label_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Common_Label",
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
                    MainImage = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "Data_ShoppingCartItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    AddedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawAttributes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data_ShoppingCartItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Data_ShoppingCartItem_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Data_ShoppingCartItem_Platform_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Platform_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Discount_ProductDiscount_Mapping",
                columns: table => new
                {
                    DiscountId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discount_ProductDiscount_Mapping", x => new { x.DiscountId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_Discount_ProductDiscount_Mapping_Catalog_Discount_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Catalog_Discount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discount_ProductDiscount_Mapping_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Catalog_DiscountUsageHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscountId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_DiscountUsageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalog_DiscountUsageHistory_Catalog_Discount_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Catalog_Discount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalog_DiscountUsageHistory_Checkout_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Checkout_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Checkout_OrderItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderItemGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    SubtotalRounded = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPriceRounded = table.Column<decimal>(type: "numeric", nullable: false),
                    RawAttributes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkout_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checkout_OrderItem_Catalog_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Catalog_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checkout_OrderItem_Checkout_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Checkout_Order",
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
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsEssential = table.Column<bool>(type: "boolean", nullable: false),
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
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "IX_Catalog_Discount_BadgeId",
                table: "Catalog_Discount",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_DiscountUsageHistory_DiscountId",
                table: "Catalog_DiscountUsageHistory",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_DiscountUsageHistory_OrderId",
                table: "Catalog_DiscountUsageHistory",
                column: "OrderId");

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
                name: "IX_Catalog_ProductBrand_BrandId",
                table: "Catalog_ProductBrand",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductBrand_ProductId",
                table: "Catalog_ProductBrand",
                column: "ProductId");

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
                name: "IX_Checkout_Order_ShippingAddressId",
                table: "Checkout_Order",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_Order_UserId",
                table: "Checkout_Order",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_OrderItem_OrderId",
                table: "Checkout_OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_OrderItem_ProductId",
                table: "Checkout_OrderItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Address_CityId",
                table: "Common_Address",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_ProductLabel_ProductId",
                table: "Common_ProductLabel",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_ProductMedia_MediaFileId",
                table: "Content_ProductMedia",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_ProductMedia_ProductId",
                table: "Content_ProductMedia",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Data_ShoppingCartItem_ProductId",
                table: "Data_ShoppingCartItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Data_ShoppingCartItem_UserId",
                table: "Data_ShoppingCartItem",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Discount_CategoryDiscount_Mapping_DiscountId",
                table: "Discount_CategoryDiscount_Mapping",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_Discount_ProductDiscount_Mapping_ProductId",
                table: "Discount_ProductDiscount_Mapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Platform_User_BillingAddressId",
                table: "Platform_User",
                column: "BillingAddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platform_User_ShippingAddressId",
                table: "Platform_User",
                column: "ShippingAddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platform_UserRole_RoleId",
                table: "Platform_UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_Address_Id",
                table: "UserAddresses",
                column: "Address_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_User_Id",
                table: "UserAddresses",
                column: "User_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Catalog_DiscountUsageHistory");

            migrationBuilder.DropTable(
                name: "Catalog_ProductAttributeOption");

            migrationBuilder.DropTable(
                name: "Catalog_ProductBrand");

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
                name: "Checkout_OrderItem");

            migrationBuilder.DropTable(
                name: "Common_ProductLabel");

            migrationBuilder.DropTable(
                name: "Content_ProductMedia");

            migrationBuilder.DropTable(
                name: "Data_ShoppingCartItem");

            migrationBuilder.DropTable(
                name: "Discount_CategoryDiscount_Mapping");

            migrationBuilder.DropTable(
                name: "Discount_ProductDiscount_Mapping");

            migrationBuilder.DropTable(
                name: "Platform_ActivityLog");

            migrationBuilder.DropTable(
                name: "Platform_ActivityLogType");

            migrationBuilder.DropTable(
                name: "Platform_EntityType");

            migrationBuilder.DropTable(
                name: "Platform_ExternalIdentityLogin");

            migrationBuilder.DropTable(
                name: "Platform_Setting");

            migrationBuilder.DropTable(
                name: "Platform_ThemeVariable");

            migrationBuilder.DropTable(
                name: "Platform_UrlRecord");

            migrationBuilder.DropTable(
                name: "Platform_UserRole");

            migrationBuilder.DropTable(
                name: "UserAddresses");

            migrationBuilder.DropTable(
                name: "Catalog_ProductAttributeOptionsSet");

            migrationBuilder.DropTable(
                name: "Catalog_SpecificationAttributeOption");

            migrationBuilder.DropTable(
                name: "Common_DeliveryTime");

            migrationBuilder.DropTable(
                name: "Catalog_ProductAttribute_Mapping");

            migrationBuilder.DropTable(
                name: "Catalog_ProductReview");

            migrationBuilder.DropTable(
                name: "Checkout_Order");

            migrationBuilder.DropTable(
                name: "Common_Label");

            migrationBuilder.DropTable(
                name: "Catalog_Category");

            migrationBuilder.DropTable(
                name: "Catalog_Discount");

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
                name: "Catalog_DiscountBadge");

            migrationBuilder.DropTable(
                name: "Catalog_Brand");

            migrationBuilder.DropTable(
                name: "Common_Address");

            migrationBuilder.DropTable(
                name: "Content_MediaFile");

            migrationBuilder.DropTable(
                name: "Common_City");
        }
    }
}
