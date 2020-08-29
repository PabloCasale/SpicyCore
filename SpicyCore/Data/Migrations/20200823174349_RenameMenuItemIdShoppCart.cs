using Microsoft.EntityFrameworkCore.Migrations;

namespace SpicyCore.Data.Migrations
{
    public partial class RenameMenuItemIdShoppCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "ShoppingCarts");

            migrationBuilder.AddColumn<int>(
                name: "MenuItemId",
                table: "ShoppingCarts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MenuItemId",
                table: "ShoppingCarts");

            migrationBuilder.AddColumn<int>(
                name: "MenuId",
                table: "ShoppingCarts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
