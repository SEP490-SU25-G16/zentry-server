using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnrollmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ClassSections_ClassSectionId1",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ClassSectionId1",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ClassSectionId1",
                table: "Schedules");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RoomId_StartDate_StartTime_EndTime",
                table: "Schedules",
                columns: new[] { "RoomId", "StartDate", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_RoomId_StartDate_StartTime_EndTime",
                table: "Schedules");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassSectionId1",
                table: "Schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ClassSectionId1",
                table: "Schedules",
                column: "ClassSectionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ClassSections_ClassSectionId1",
                table: "Schedules",
                column: "ClassSectionId1",
                principalTable: "ClassSections",
                principalColumn: "Id");
        }
    }
}
