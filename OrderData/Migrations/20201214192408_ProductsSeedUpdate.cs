using Microsoft.EntityFrameworkCore.Migrations;

namespace OrderData.Migrations
{
    public partial class ProductsSeedUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "Price",
                value: 1.99);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "Price",
                value: 2.98);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "Price",
                value: 3.9700000000000002);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "Price",
                value: 1.0);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "Price",
                value: 2.0);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "Price",
                value: 3.0);
        }
    }
}
