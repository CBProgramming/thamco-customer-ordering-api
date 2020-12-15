using Microsoft.EntityFrameworkCore.Migrations;

namespace OrderData.Migrations
{
    public partial class ProductsSeeded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "customeraccount",
                newName: "Products",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "customeraccount",
                newName: "Orders",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "OrderedItems",
                schema: "customeraccount",
                newName: "OrderedItems",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "customeraccount",
                newName: "Customers",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "BasketItems",
                schema: "customeraccount",
                newName: "BasketItems",
                newSchema: "ordering");

            migrationBuilder.InsertData(
                schema: "ordering",
                table: "Products",
                columns: new[] { "ProductId", "Name", "Price" },
                values: new object[] { 1, "Fake Product 1", 1.0 });

            migrationBuilder.InsertData(
                schema: "ordering",
                table: "Products",
                columns: new[] { "ProductId", "Name", "Price" },
                values: new object[] { 2, "Fake Product 2", 2.0 });

            migrationBuilder.InsertData(
                schema: "ordering",
                table: "Products",
                columns: new[] { "ProductId", "Name", "Price" },
                values: new object[] { 3, "Fake Product 3", 3.0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);

            migrationBuilder.EnsureSchema(
                name: "customeraccount");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "ordering",
                newName: "Products",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "ordering",
                newName: "Orders",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "OrderedItems",
                schema: "ordering",
                newName: "OrderedItems",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "ordering",
                newName: "Customers",
                newSchema: "customeraccount");

            migrationBuilder.RenameTable(
                name: "BasketItems",
                schema: "ordering",
                newName: "BasketItems",
                newSchema: "customeraccount");
        }
    }
}
