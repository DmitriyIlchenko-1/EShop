using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class _6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Data_ShoppingCartItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Data_ShoppingCartItem_UserId",
                table: "Data_ShoppingCartItem",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Data_ShoppingCartItem_Platform_User_UserId",
                table: "Data_ShoppingCartItem",
                column: "UserId",
                principalTable: "Platform_User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Data_ShoppingCartItem_Platform_User_UserId",
                table: "Data_ShoppingCartItem");

            migrationBuilder.DropIndex(
                name: "IX_Data_ShoppingCartItem_UserId",
                table: "Data_ShoppingCartItem");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Data_ShoppingCartItem");
        }
    }
}
