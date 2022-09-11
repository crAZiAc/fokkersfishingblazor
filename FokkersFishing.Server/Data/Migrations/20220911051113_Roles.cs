using Microsoft.EntityFrameworkCore.Migrations;

namespace FokkersFishing.Data.Migrations
{
    public partial class Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "b02fe58d-6f0c-4b73-80de-107284940bee", "f8ddfd40-13ad-4f24-a0cb-909c5ff6545b", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "17c56128-b8bf-4edb-b619-9387cc664c58", "de74e57c-8f64-4746-9f1f-c414aac7fd64", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "17c56128-b8bf-4edb-b619-9387cc664c58");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b02fe58d-6f0c-4b73-80de-107284940bee");
        }
    }
}
