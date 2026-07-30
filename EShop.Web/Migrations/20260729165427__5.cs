using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class _5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catalog_Product_Catalog_Brand_BrandId",
                table: "Catalog_Product");

            migrationBuilder.RenameColumn(
                name: "SubtotalWithNoDiscount",
                table: "Checkout_OrderItem",
                newName: "UnitPriceRounded");

            migrationBuilder.RenameColumn(
                name: "SubtotalWithDiscount",
                table: "Checkout_OrderItem",
                newName: "SubtotalRounded");

            migrationBuilder.RenameColumn(
                name: "OrderSubtotalWithNoDiscount",
                table: "Checkout_Order",
                newName: "SubtotalRounded");

            migrationBuilder.RenameColumn(
                name: "OrderSubtotalWithDiscount",
                table: "Checkout_Order",
                newName: "Subtotal");

            migrationBuilder.AddColumn<bool>(
                name: "MainImage",
                table: "Content_ProductMedia",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Checkout_OrderItem",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

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

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_OrderItem_ProductId",
                table: "Checkout_OrderItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductBrand_BrandId",
                table: "Catalog_ProductBrand",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_ProductBrand_ProductId",
                table: "Catalog_ProductBrand",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Catalog_Product_Catalog_Brand_BrandId",
                table: "Catalog_Product",
                column: "BrandId",
                principalTable: "Catalog_Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Checkout_OrderItem_Catalog_Product_ProductId",
                table: "Checkout_OrderItem",
                column: "ProductId",
                principalTable: "Catalog_Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catalog_Product_Catalog_Brand_BrandId",
                table: "Catalog_Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Checkout_OrderItem_Catalog_Product_ProductId",
                table: "Checkout_OrderItem");

            migrationBuilder.DropTable(
                name: "Catalog_ProductBrand");

            migrationBuilder.DropIndex(
                name: "IX_Checkout_OrderItem_ProductId",
                table: "Checkout_OrderItem");

            migrationBuilder.DropColumn(
                name: "MainImage",
                table: "Content_ProductMedia");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Checkout_OrderItem");

            migrationBuilder.RenameColumn(
                name: "UnitPriceRounded",
                table: "Checkout_OrderItem",
                newName: "SubtotalWithNoDiscount");

            migrationBuilder.RenameColumn(
                name: "SubtotalRounded",
                table: "Checkout_OrderItem",
                newName: "SubtotalWithDiscount");

            migrationBuilder.RenameColumn(
                name: "SubtotalRounded",
                table: "Checkout_Order",
                newName: "OrderSubtotalWithNoDiscount");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "Checkout_Order",
                newName: "OrderSubtotalWithDiscount");

            migrationBuilder.AddForeignKey(
                name: "FK_Catalog_Product_Catalog_Brand_BrandId",
                table: "Catalog_Product",
                column: "BrandId",
                principalTable: "Catalog_Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
