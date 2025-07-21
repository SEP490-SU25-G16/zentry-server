using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityClassSectionAndUpdateOtherEntitiesRelated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Schedules_ScheduleId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_CourseId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "LecturerId",
                table: "Schedules",
                newName: "ClassSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_LecturerId",
                table: "Schedules",
                newName: "IX_Schedules_ClassSectionId");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "Enrollments",
                newName: "ClassSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId_ScheduleId",
                table: "Enrollments",
                newName: "IX_Enrollments_StudentId_ClassSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_ScheduleId",
                table: "Enrollments",
                newName: "IX_Enrollments_ClassSectionId");

            migrationBuilder.CreateTable(
                name: "ClassSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    LecturerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Semester = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSections_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_CourseId",
                table: "ClassSections",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_LecturerId",
                table: "ClassSections",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_SectionCode",
                table: "ClassSections",
                column: "SectionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_Semester",
                table: "ClassSections",
                column: "Semester");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_ClassSections_ClassSectionId",
                table: "Enrollments",
                column: "ClassSectionId",
                principalTable: "ClassSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ClassSections_ClassSectionId",
                table: "Schedules",
                column: "ClassSectionId",
                principalTable: "ClassSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_ClassSections_ClassSectionId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ClassSections_ClassSectionId",
                table: "Schedules");

            migrationBuilder.DropTable(
                name: "ClassSections");

            migrationBuilder.RenameColumn(
                name: "ClassSectionId",
                table: "Schedules",
                newName: "LecturerId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_ClassSectionId",
                table: "Schedules",
                newName: "IX_Schedules_LecturerId");

            migrationBuilder.RenameColumn(
                name: "ClassSectionId",
                table: "Enrollments",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId_ClassSectionId",
                table: "Enrollments",
                newName: "IX_Enrollments_StudentId_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_ClassSectionId",
                table: "Enrollments",
                newName: "IX_Enrollments_ScheduleId");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CourseId",
                table: "Schedules",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Schedules_ScheduleId",
                table: "Enrollments",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
