using Microsoft.EntityFrameworkCore.Migrations;

namespace LearnDotNetCore.Migrations
{
    public partial class PhotoNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Department", "Email", "Name", "PhotoPath" },
                values: new object[] { -1, 2, "autoseed@smarttech.com", "Auto Seed", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}
