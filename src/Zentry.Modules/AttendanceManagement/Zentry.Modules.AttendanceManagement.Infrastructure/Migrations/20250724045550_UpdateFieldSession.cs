using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAbsent",
                table: "AttendanceRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAbsent",
                table: "AttendanceRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
