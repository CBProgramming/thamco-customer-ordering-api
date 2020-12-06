using Microsoft.EntityFrameworkCore.Migrations;

namespace OrderData.Migrations
{
    public partial class schemaAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customeraccount");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Orders",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "OrderedItems",
                newName: "OrderedItems",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Customers",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "BasketItems",
                newName: "BasketItems",
                newSchema: "customeraccount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Products",
                schema: "customeraccount",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "customeraccount",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderedItems",
                schema: "customeraccount",
                newName: "OrderedItems");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "customeraccount",
                newName: "Customers");

            migrationBuilder.RenameTable(
                name: "BasketItems",
                schema: "customeraccount",
                newName: "BasketItems");
        }
    }
}
