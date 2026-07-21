using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class _9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Common_Address",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Common_Address_UserId",
                table: "Common_Address",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Common_Address_Platform_User_UserId",
                table: "Common_Address",
                column: "UserId",
                principalTable: "Platform_User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Common_Address_Platform_User_UserId",
                table: "Common_Address");

            migrationBuilder.DropIndex(
                name: "IX_Common_Address_UserId",
                table: "Common_Address");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Common_Address");
        }
    }
}
