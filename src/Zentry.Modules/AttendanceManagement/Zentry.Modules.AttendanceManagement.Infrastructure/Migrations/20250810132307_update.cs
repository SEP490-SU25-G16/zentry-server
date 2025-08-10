using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AttendanceRecords",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_UserId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "AttendanceRecords",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_UserId");
        }
    }
}
