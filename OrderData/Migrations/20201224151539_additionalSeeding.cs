using Microsoft.EntityFrameworkCore.Migrations;

namespace OrderData.Migrations
{
    public partial class additionalSeeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                columns: new[] { "Country", "CustomerAuthId" },
                values: new object[] { "UK", "f756701c-4336-47b1-8317-a16e84bd0059" });

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "Quantity",
                value: 20);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "Quantity",
                value: 20);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "Quantity",
                value: 20);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                columns: new[] { "Country", "CustomerAuthId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "Quantity",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "Quantity",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "ordering",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "Quantity",
                value: 0);
        }
    }
}
