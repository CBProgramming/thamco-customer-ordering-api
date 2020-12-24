using Microsoft.EntityFrameworkCore.Migrations;

namespace OrderData.Migrations
{
    public partial class dataStructureFinalised : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                schema: "ordering",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomerAuthId",
                schema: "ordering",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "ordering",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomerAuthId",
                schema: "ordering",
                table: "Customers");
        }
    }
}
