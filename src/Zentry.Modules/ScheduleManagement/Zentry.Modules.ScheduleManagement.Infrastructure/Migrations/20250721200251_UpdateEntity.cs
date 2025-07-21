using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassSectionId1",
                table: "Schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassSectionId1",
                table: "Enrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ClassSectionId1",
                table: "Schedules",
                column: "ClassSectionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassSectionId1",
                table: "Enrollments",
                column: "ClassSectionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_ClassSections_ClassSectionId1",
                table: "Enrollments",
                column: "ClassSectionId1",
                principalTable: "ClassSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ClassSections_ClassSectionId1",
                table: "Schedules",
                column: "ClassSectionId1",
                principalTable: "ClassSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_ClassSections_ClassSectionId1",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ClassSections_ClassSectionId1",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ClassSectionId1",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_ClassSectionId1",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "ClassSectionId1",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ClassSectionId1",
                table: "Enrollments");
        }
    }
}
