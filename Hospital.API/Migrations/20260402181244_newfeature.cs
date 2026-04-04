using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hospital.API.Migrations
{
    /// <inheritdoc />
    public partial class newfeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NightShiftTeams",
                columns: new[] { "Id", "SupervisorId" },
                values: new object[,]
                {
                    { 1, null },
                    { 2, null },
                    { 3, null },
                    { 4, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NightShiftTeams",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NightShiftTeams",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NightShiftTeams",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NightShiftTeams",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
