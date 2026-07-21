using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Content_MediaFile",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Catalog_Reply",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Catalog_ProductReview",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Published",
                table: "Catalog_Product",
                newName: "IsPublished");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Catalog_Product",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Catalog_Discount",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Catalog_Category",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Catalog_Brand",
                newName: "IsDeleted");

            migrationBuilder.AlterColumn<int>(
                name: "MaxAddToCartNumber",
                table: "Catalog_Product",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAddToCartNumber",
                table: "Catalog_Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinAddToCartNumber",
                table: "Catalog_Product");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Content_MediaFile",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Catalog_Reply",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Catalog_ProductReview",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "Catalog_Product",
                newName: "Published");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Catalog_Product",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Catalog_Discount",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Catalog_Category",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Catalog_Brand",
                newName: "Deleted");

            migrationBuilder.AlterColumn<int>(
                name: "MaxAddToCartNumber",
                table: "Catalog_Product",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
