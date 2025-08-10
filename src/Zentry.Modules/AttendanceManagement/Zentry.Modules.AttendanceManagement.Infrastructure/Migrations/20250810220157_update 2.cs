using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionNumber",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionNumber",
                table: "Sessions",
                column: "SessionNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Sessions_SessionId",
                table: "AttendanceRecords",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Sessions_SessionId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SessionNumber",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SessionNumber",
                table: "Sessions");
        }
    }
}
