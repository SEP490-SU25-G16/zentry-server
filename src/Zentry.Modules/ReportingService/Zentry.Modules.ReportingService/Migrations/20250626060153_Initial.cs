using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ReportingService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportContent = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceReports_CreatedAt",
                table: "AttendanceReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceReports_ReportType",
                table: "AttendanceReports",
                column: "ReportType");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceReports_ScopeId",
                table: "AttendanceReports",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceReports_ScopeType",
                table: "AttendanceReports",
                column: "ScopeType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceReports");
        }
    }
}
