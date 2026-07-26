using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class _3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponUsageAmount",
                table: "Catalog_Discount");

            migrationBuilder.RenameColumn(
                name: "DiscountUsageAmount",
                table: "Catalog_Discount",
                newName: "AppliedTimes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppliedTimes",
                table: "Catalog_Discount",
                newName: "DiscountUsageAmount");

            migrationBuilder.AddColumn<int>(
                name: "CouponUsageAmount",
                table: "Catalog_Discount",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
