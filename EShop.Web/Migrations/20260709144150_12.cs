using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class _12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Content_MediaFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Content_MediaFile",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
