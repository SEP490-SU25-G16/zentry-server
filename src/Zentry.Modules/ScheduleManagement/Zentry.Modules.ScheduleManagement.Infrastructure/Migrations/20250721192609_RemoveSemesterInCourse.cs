using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSemesterInCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_Semester",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "Courses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Semester",
                table: "Courses",
                column: "Semester");
        }
    }
}
