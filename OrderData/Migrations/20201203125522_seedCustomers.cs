using Microsoft.EntityFrameworkCore.Migrations;

namespace OrderData.Migrations
{
    public partial class seedCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Active", "AddressOne", "AddressTwo", "AreaCode", "CanPurchase", "Country", "EmailAddress", "FamilyName", "GivenName", "State", "TelephoneNumber", "Town" },
                values: new object[] { 1, true, "85 Clifton Road", null, "DL1 5RT", true, null, "t7145969@live.tees.ac.uk", "Burrell", "Chris", "Durham", "09876543210", "Downtown" });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Active", "AddressOne", "AddressTwo", "AreaCode", "CanPurchase", "Country", "EmailAddress", "FamilyName", "GivenName", "State", "TelephoneNumber", "Town" },
                values: new object[] { 2, true, "20 Fake Road", null, "DLF AKE", true, null, "fake@live.tees.ac.uk", "McFakeFace", "Fakie", "FakeState", "01010101010", "FakeTown" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2);
        }
    }
}
