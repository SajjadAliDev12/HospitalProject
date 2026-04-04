using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.API.Migrations
{
    /// <inheritdoc />
    public partial class updateEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NightShiftId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "enMorningGroup",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NightShiftTeam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupervisorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NightShiftTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NightShiftTeam_Employees_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NightShiftId",
                table: "Employees",
                column: "NightShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_NightShiftTeam_SupervisorId",
                table: "NightShiftTeam",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_NightShiftTeam_NightShiftId",
                table: "Employees",
                column: "NightShiftId",
                principalTable: "NightShiftTeam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_NightShiftTeam_NightShiftId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "NightShiftTeam");

            migrationBuilder.DropIndex(
                name: "IX_Employees_NightShiftId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NightShiftId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "enMorningGroup",
                table: "Employees");
        }
    }
}
