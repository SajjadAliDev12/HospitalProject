using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.API.Migrations
{
    /// <inheritdoc />
    public partial class addsystemsetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_NightShiftTeam_NightShiftId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_NightShiftTeam_Employees_SupervisorId",
                table: "NightShiftTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NightShiftTeam",
                table: "NightShiftTeam");

            migrationBuilder.RenameTable(
                name: "NightShiftTeam",
                newName: "NightShiftTeams");

            migrationBuilder.RenameIndex(
                name: "IX_NightShiftTeam_SupervisorId",
                table: "NightShiftTeams",
                newName: "IX_NightShiftTeams_SupervisorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NightShiftTeams",
                table: "NightShiftTeams",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftReferenceDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_NightShiftTeams_NightShiftId",
                table: "Employees",
                column: "NightShiftId",
                principalTable: "NightShiftTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NightShiftTeams_Employees_SupervisorId",
                table: "NightShiftTeams",
                column: "SupervisorId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_NightShiftTeams_NightShiftId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_NightShiftTeams_Employees_SupervisorId",
                table: "NightShiftTeams");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NightShiftTeams",
                table: "NightShiftTeams");

            migrationBuilder.RenameTable(
                name: "NightShiftTeams",
                newName: "NightShiftTeam");

            migrationBuilder.RenameIndex(
                name: "IX_NightShiftTeams_SupervisorId",
                table: "NightShiftTeam",
                newName: "IX_NightShiftTeam_SupervisorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NightShiftTeam",
                table: "NightShiftTeam",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_NightShiftTeam_NightShiftId",
                table: "Employees",
                column: "NightShiftId",
                principalTable: "NightShiftTeam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NightShiftTeam_Employees_SupervisorId",
                table: "NightShiftTeam",
                column: "SupervisorId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
